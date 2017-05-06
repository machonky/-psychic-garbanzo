using System;
using System.Threading;
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

                var joinNetworkReply = new QueryJoinNetworkReply(_node.Identity, message.From, message.CorrelationId)
                {
                    RoutingTable = new RoutingTableEntry[0],
                };

                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        GetSuccessorTable(successorCorrelation, message.From);
                        GetRoutingTable(routingCorrelation, message.From);
                    })
                    .AndAwait(successorCorrelation, (GetSuccessorTableReply successorTableReply) =>
                    {
                        _node.SendAckMessage(message, message.CorrelationId);
                        joinNetworkReply.SuccessorTable = successorTableReply.SuccessorTable;
                    })
                    //.AndAwait(routingCorrelation, (GetRoutingTableReply routingTableReply) =>
                    //{
                    //    _node.SendAckMessage(message, message.CorrelationId);
                    //    joinNetworkReply.RoutingTable = routingTableReply.RoutingTable;
                    //})
                    .ContinueWith(() =>
                    {
                        var replySocket = _node.ForwardingSockets[message.From.HostAndPort];
                        _node.Marshaller.Send(joinNetworkReply, replySocket);
                    })
                    .Run(message.CorrelationId);
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

            private void GetSuccessorTable(CorrelationId operationId, NodeInfo applicantInfo)
            {
                var reply = new GetSuccessorTableReply(_node.Identity, applicantInfo, operationId)
                {
                    SuccessorTable = new RoutingTableEntry[_node.Config.SuccessorCount],
                };

                var timeout = _node.Config.AwaitSettings.AwaitTimeout == Timeout.Infinite
                    ? Timeout.Infinite
                    : _node.Config.SuccessorCount*_node.Config.AwaitSettings.AwaitTimeout;

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
                        reply.SuccessorTable = successorReply.SuccessorTable;
                    })
                    .ContinueWith(() =>
                    {
                        // Merge the result back with the parent query
                        _node.Marshaller.Send(reply, _node.Actor);
                    })
                    .Run(operationId, timeout);
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

            private void GetRoutingTable(CorrelationId operationId, NodeInfo applicantInfo)
            {
                
            }
        }
    }
}
