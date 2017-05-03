using System;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class JoinNetworkHandler
            : IHandle<QueryJoinNetwork>
            , IHandle<GetSuccessor>
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
                _node.SendAckMessage(message, message.CorrelationId);

                var routingCorrelation = GetNextCorrelation();
                var successorCorrelation = GetNextCorrelation();

                var joinNetworkReply = new QueryJoinNetworkReply(_node.Identity, message.From, message.CorrelationId);

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
                var operationCount = _node.Config.SuccessorCount;
                var reply = new GetSuccessorTableReply(_node.Identity, applicantInfo, operationId)
                {
                    SuccessorTable = new RoutingTableEntry[operationCount]
                };

                var opIds = GetOperationIds(operationCount);
                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler
                    .PerformAction(() =>
                    {
                        for (int i = 0; i < operationCount; ++i)
                        {
                            var opId = opIds[i];
                            var getSuccessor = new GetSuccessor(_node.Identity, _node.Identity, opId)
                            {
                                Applicant = applicantInfo,
                                HopCount = i,
                            };
                        }
                    })
                    .AndAwaitAll(opIds, (GetSuccessorReply successorReply) =>
                    {
                        reply.SuccessorTable[successorReply.SuccessorIndex] =
                            new RoutingTableEntry(successorReply.Successor.RoutingHash, successorReply.Successor);
                    })
                    .ContinueWith(() =>
                    {
                        _node.Marshaller.Send(reply, _node.Actor);
                    })
                    .Run(operationId);
            }

            public void Handle(GetSuccessor message)
            {
                var applicant = message.Applicant;
                if (_node.IsInDomain(applicant.RoutingHash))
                {
                    // this node is the successor + (r-1) of this nodes successor list.
                    var reply = new GetSuccessorReply(_node.Identity, message.From, message.CorrelationId)
                    {
                        Successor = _node.Identity,
                        SuccessorIndex = message.HopCount,
                    };
                    // Send the reply
                }
                else // Ask the network
                {
                    var closestNode = _node.FingerTable.FindClosestPrecedingFinger(applicant.RoutingHash);
                    var closestSocket = _node.ForwardingSockets[closestNode.HostAndPort];
                    _node.Marshaller.Send(msg, closestSocket);
                }
            }

            private void GetRoutingTable(CorrelationId operationId, NodeInfo applicantInfo)
            {
                
            }

        }
    }
}
