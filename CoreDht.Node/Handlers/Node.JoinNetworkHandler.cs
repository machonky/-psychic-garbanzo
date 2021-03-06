﻿using System;
using System.Linq;
using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class JoinNetworkHandler
            : IHandle<InitJoin>
            , IHandle<JoinNetwork>
            , IHandle<JoinToSeed>
            , IHandle<GetSuccessorTable>
            , IHandle<GetRoutingEntry>
        {
            private readonly Node _node;
            private readonly ICommunicationManager _commMgr;
            private Func<CorrelationId> GetNextCorrelation { get; }

            public JoinNetworkHandler(Node node, ICommunicationManager commMgr)
            {
                _node = node;
                _commMgr = commMgr;
                GetNextCorrelation = _node.Config.CorrelationFactory.GetNextCorrelation;
            }

            public void Handle(InitJoin message)
            {
                _node.LogMessage(message);

                var seedNode = _node.Config.SeedNode;
                var seedNodeInfo = _node.CalcNodeInfo(seedNode);

                // this needs to be on a retry mechanism
                JoinToSeed(seedNodeInfo);
                //_commMgr.SendInternal(new JoinToSeed {SeedNode = seedNodeInfo,});
            }

            public void Handle(JoinToSeed message)
            {
                _node.Log($"{message.TypeName()} SeedNode:{message.SeedNode}");
                JoinToSeed(message.SeedNode);
            }

            private void JoinToSeed(NodeInfo seedNodeInfo)
            {
                _node.Predecessor = _node.Identity;
                _node.FingerTable.Init();

                var opId = _node.Config.CorrelationFactory.GetNextCorrelation();
                var startTime = _node.Clock.Now;

                var responseHandler = _node.CreateAwaitAllResponsesHandler();
                responseHandler
                    .PerformAction(() =>
                    {
                        var msg = new JoinNetwork(_node.Identity, seedNodeInfo, opId)
                        {
                            RoutingTable = _node.FingerTable.Entries,
                        };

                        _node.Log($"Sending {msg.TypeName()} To {seedNodeInfo} Id:{opId}");
                        _commMgr.Send(msg);
                    })
                    .AndAwait(opId, (JoinNetworkReply reply) =>
                    {
                        _node.Log($"{reply.TypeName()} From {reply.From} Id:{reply.CorrelationId}");
                        _node.Log($"Join took {(_node.Clock.Now - startTime).Milliseconds} ms");
                        _node.Log($"Assigning successor {reply.SuccessorTable[0].SuccessorIdentity}");

                        _node.SuccessorTable.Copy(reply.SuccessorTable);
                        //_node.FingerTable.Copy(reply.RoutingTable);

                        // This node has "joined" but is not in an ideal state as it is not part of the ring network yet.
                    })
                    .ContinueWith(() =>
                    {
                        _commMgr.SendInternal(new InitStabilize());
                    })
                    .Run(opId);
            }


            public void Handle(JoinNetwork message)
            {
                _node.Log($"{message.TypeName()} From:{message.From}");
                _commMgr.SendAck(message, message.CorrelationId);

                var routingCorrelation = GetNextCorrelation();
                var successorCorrelation = GetNextCorrelation();

                var joinNetworkReply = new JoinNetworkReply(_node.Identity, message.From, message.CorrelationId);

                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        GetSuccessorTable(successorCorrelation, message.From);
                        //GetRoutingTable(routingCorrelation, message.From, message.RoutingTable);
                    })
                    .AndAwait(successorCorrelation, (GetSuccessorTableReply successorTableReply) =>
                    {
                        _commMgr.SendAck(message, message.CorrelationId);
                        joinNetworkReply.SuccessorTable = successorTableReply.SuccessorTable;
                    })
                    //.AndAwait(routingCorrelation, (GetRoutingTableReply routingTableReply) =>
                    //{
                    //    _commMgr.SendAck(message, message.CorrelationId);
                    //    joinNetworkReply.RoutingTable = routingTableReply.RoutingTable;
                    //})
                    .ContinueWith(() =>
                    {
                        _commMgr.Send(joinNetworkReply);
                    })
                    .Run(message.CorrelationId);
            }

            private void GetSuccessorTable(CorrelationId operationId, NodeInfo applicantInfo)
            {
                var assembledReply = new GetSuccessorTableReply(_node.Identity, applicantInfo, operationId)
                {
                    SuccessorTable = new RoutingTableEntry[_node.Config.SuccessorCount],
                };

                var totalTimeout = _node.Config.SuccessorCount*_node.Config.AwaitSettings.NetworkQueryTimeout;

                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        var thisNode = _node.Identity;
                        var initialMessage = new GetSuccessorTable(thisNode, thisNode, operationId)
                        {
                            Applicant = applicantInfo,
                            SuccessorTable = new RoutingTableEntry[_node.Config.SuccessorCount],
                            HopCount = 0,
                        };

                        _node.Log($"Sending {initialMessage.TypeName()} To {initialMessage.To} Id:{initialMessage.CorrelationId}");
                        _commMgr.SendInternal(initialMessage);
                    })
                    .AndAwait(operationId, (GetSuccessorTableReply successorReply) =>
                    {
                        assembledReply.SuccessorTable = successorReply.SuccessorTable;
                    })
                    .ContinueWith(() =>
                    {
                        // Merge the result back with the parent query
                        _commMgr.SendInternal(assembledReply);
                    })
                    .Run(operationId, totalTimeout);
            }

            public void Handle(GetSuccessorTable message)
            {
                _node.LogMessage(message);
                _commMgr.SendAck(message, message.CorrelationId);

                var applicant = message.Applicant;
                if (_node.IsInDomain(applicant.RoutingHash))
                {
                    _node.Log($"{applicant} IsInDomain");
                    // the predecessors successor table will become the applicant's if the applicant node
                    // becomes the new predecessor. The head of the predecessors table is this node.

                    var nextSuccessor = _node.Identity;
                    var returnAddress = message.From;
                    var tableLength = _node.SuccessorTable.Length;
                    var predecessorTable = new RoutingTableEntry[_node.SuccessorTable.Length];
                    if (tableLength > 1)
                    {
                        var thisTable = _node.SuccessorTable;
                        for (int i = 0; i < predecessorTable.Length - 1; i++)
                        {
                            predecessorTable[i + 1] = thisTable[i];
                        }
                    }

                    predecessorTable[0] = new RoutingTableEntry(_node.Identity.RoutingHash, _node.Identity);

                    var reply = new GetSuccessorTableReply(_node.Identity, returnAddress, message.CorrelationId)
                    {
                        SuccessorTable = predecessorTable,
                    };
                    _node.Log($"Sending {reply.TypeName()}");
                    _commMgr.Send(reply);


                    //message.SuccessorTable[message.HopCount] = new RoutingTableEntry(nextSuccessor.RoutingHash, nextSuccessor);

                    //var hopMax = _node.Config.SuccessorCount - 1;
                    //if (message.HopCount < hopMax)
                    //{
                    //    nextSuccessor = _node.Successor;
                    //    _node.Log($"Successor({message.HopCount}) found. Forwarding to {nextSuccessor}");

                    //    var forwardMsg = message;
                    //    forwardMsg.To = nextSuccessor;
                    //    forwardMsg.HopCount++;
                    //    forwardMsg.Applicant = nextSuccessor; // we need to now follow a chain of successors

                    //    _commMgr.Send(forwardMsg);
                    //}
                    //else
                    //{
                    //    // we've collected all the successors. Now send them home
                    //    var returnAddress = message.From;
                    //    var reply = new GetSuccessorTableReply(_node.Identity, returnAddress, message.CorrelationId)
                    //    {
                    //        SuccessorTable = message.SuccessorTable,
                    //    };
                    //    _node.Log($"Sending {reply.TypeName()} Successor({message.HopCount}) found.");
                    //    _commMgr.Send(reply);
                    //}
                }
                else // Ask the network
                {
                    var closestNode = _node.SuccessorTable.FindClosestPrecedingFinger(applicant.RoutingHash);
                    message.To = closestNode;
                    _node.Log($"Routing {message.TypeName()} {message.To}");
                    _commMgr.Send(message);
                }
            }

            private CorrelationId[] GetOperationIds(int operationCount)
            {
                var opIds = new CorrelationId[operationCount];
                for (int i = 0; i < operationCount; ++i)
                {
                    opIds[i] = GetNextCorrelation();
                }
                return opIds;
            }

            private void GetRoutingTable(CorrelationId operationId, NodeInfo applicantInfo, RoutingTableEntry[] routingTable)
            {
                var totalTimeout = routingTable.Length*_node.Config.AwaitSettings.NetworkQueryTimeout;
                var assembledReply = new GetRoutingTableReply(_node.Identity, applicantInfo, operationId)
                {
                    RoutingTable = routingTable,
                };

                var networkResults = routingTable.ToDictionary(x => x.StartValue);
                var queryIds = GetOperationIds(routingTable.Length);
                var identity = _node.Identity;

                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        for (int i = 0; i < routingTable.Length; i++)
                        {
                            var entryQuery = new GetRoutingEntry(identity, identity, queryIds[i])
                            {
                                StartValue = routingTable[i].StartValue,
                            };

                            _commMgr.SendInternal(entryQuery);
                        }
                    })
                    .AndAwaitAll(queryIds, (GetRoutingEntryReply entryReply) =>
                    {
                        networkResults[entryReply.Entry.StartValue] = entryReply.Entry;
                    })
                    .ContinueWith(() =>
                    {
                        var replyTable = assembledReply.RoutingTable;
                        var tableLength = replyTable.Length;

                        for (int i = 0; i < tableLength; ++i)
                        {
                            var startValue = replyTable[i].StartValue;
                            replyTable[i] = networkResults[startValue];
                        }

                        _commMgr.SendInternal(assembledReply);
                    })
                    .Run(operationId, totalTimeout);
            }

            public void Handle(GetRoutingEntry message)
            {
                _commMgr.SendAck(message, message.CorrelationId);

                if (_node.IsInDomain(message.StartValue))
                {
                    var reply = new GetRoutingEntryReply(_node.Identity, message.From, message.CorrelationId)
                    {
                        Entry = new RoutingTableEntry(message.StartValue, _node.Successor),
                    };

                    _commMgr.SendInternal(reply);
                }
                else
                {
                    var closestNode = _node.FingerTable.FindClosestPrecedingFinger(message.StartValue);
                    message.To = closestNode;

                    _commMgr.Send(message);
                }
            }
        }
    }
}
