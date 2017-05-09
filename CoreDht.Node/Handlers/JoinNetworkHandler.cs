using System;
using System.Linq;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class JoinNetworkHandler
            : IHandle<QueryJoinNetwork>
            , IHandle<GetSuccessorTable>
            , IHandle<GetRoutingEntry>
        {
            private readonly Node _node;
            private Func<CorrelationId> GetNextCorrelation { get; }

            public JoinNetworkHandler(Node node)
            {
                _node = node;
                GetNextCorrelation = _node.Config.CorrelationFactory.GetNextCorrelation;
            }

            public void Handle(QueryJoinNetwork message)
            {
                _node.Log($"Received QueryJoinNetwork from {message.From}");
                _node.SendAckMessage(message, message.CorrelationId);

                var routingCorrelation = GetNextCorrelation();
                var successorCorrelation = GetNextCorrelation();

                var joinNetworkReply = new QueryJoinNetworkReply(_node.Identity, message.From, message.CorrelationId);

                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        GetSuccessorTable(successorCorrelation, message.From);
                        GetRoutingTable(routingCorrelation, message.From, message.RoutingTable);
                    })
                    .AndAwait(successorCorrelation, (GetSuccessorTableReply successorTableReply) =>
                    {
                        _node.SendAckMessage(message, message.CorrelationId);
                        joinNetworkReply.SuccessorTable = successorTableReply.SuccessorTable;
                    })
                    .AndAwait(routingCorrelation, (GetRoutingTableReply routingTableReply) =>
                    {
                        _node.SendAckMessage(message, message.CorrelationId);
                        joinNetworkReply.RoutingTable = routingTableReply.RoutingTable;
                    })
                    .ContinueWith(() =>
                    {
                        var replySocket = _node.ForwardingSockets[message.From.HostAndPort];
                        _node.Marshaller.Send(joinNetworkReply, replySocket);
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

                        var outgoingSocket = _node.ForwardingSockets[thisNode.HostAndPort];
                        _node.Marshaller.Send(initialMessage, outgoingSocket);
                    })
                    .AndAwait(operationId, (GetSuccessorTableReply successorReply) =>
                    {
                        assembledReply.SuccessorTable = successorReply.SuccessorTable;
                    })
                    .ContinueWith(() =>
                    {
                        // Merge the result back with the parent query
                        _node.Marshaller.Send(assembledReply, _node.Actor);
                    })
                    .Run(operationId, totalTimeout);
            }

            public void Handle(GetSuccessorTable message)
            {
                _node.SendAckMessage(message, message.CorrelationId);

                var applicant = message.Applicant;
                if (_node.IsInDomain(applicant.RoutingHash))
                {
                    var nextSuccessor = _node.Successor;
                    message.SuccessorTable[message.HopCount] = new RoutingTableEntry(nextSuccessor.RoutingHash, nextSuccessor);

                    var hopMax = _node.Config.SuccessorCount - 1;
                    if (message.HopCount < hopMax)
                    {
                        _node.Log($"Successor({message.HopCount}) found. Forwarding to {nextSuccessor}");

                        var forwardMsg = message;
                        forwardMsg.To = nextSuccessor;
                        forwardMsg.HopCount++;
                        forwardMsg.Applicant = nextSuccessor; // we need to now follow a chain of successors

                        var successorSocket = _node.ForwardingSockets[nextSuccessor.HostAndPort];
                        _node.Marshaller.Send(forwardMsg, successorSocket);
                    }
                    else
                    {
                        // we've collected all the successors. Now send them home
                        var returnAddress = message.From;
                        _node.Log($"Successor({message.HopCount}) found. Reply to {returnAddress}");
                        var reply = new GetSuccessorTableReply(_node.Identity, returnAddress, message.CorrelationId)
                        {
                            SuccessorTable = message.SuccessorTable,
                        };
                        var returnSocket = _node.ForwardingSockets[returnAddress.HostAndPort];
                        _node.Marshaller.Send(reply, returnSocket);
                    }
                }
                else // Ask the network
                {
                    var closestNode = _node.FingerTable.FindClosestPrecedingFinger(applicant.RoutingHash);
                    message.To = closestNode;

                    var closestSocket = _node.ForwardingSockets[closestNode.HostAndPort];
                    _node.Marshaller.Send(message, closestSocket);
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

                            var socket = _node.ForwardingSockets[identity.HostAndPort];
                            _node.Marshaller.Send(entryQuery, socket);
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
                        _node.Marshaller.Send(assembledReply, _node.Actor);
                    })
                    .Run(operationId, totalTimeout);
            }

            public void Handle(GetRoutingEntry message)
            {
                _node.SendAckMessage(message, message.CorrelationId);

                if (_node.IsInDomain(message.StartValue))
                {
                    var reply = new GetRoutingEntryReply(_node.Identity, message.From, message.CorrelationId)
                    {
                        Entry = new RoutingTableEntry(message.StartValue, _node.Successor),
                    };

                    var socket = _node.ForwardingSockets[_node.Identity.HostAndPort];
                    _node.Marshaller.Send(reply, socket);
                }
                else
                {
                    var closestNode = _node.FingerTable.FindClosestPrecedingFinger(message.StartValue);
                    var socket = _node.ForwardingSockets[closestNode.HostAndPort];
                    _node.Marshaller.Send(message, socket);
                }
            }
        }
    }
}
