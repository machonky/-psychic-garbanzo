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

            private void GetSuccessorTable(CorrelationId operationId, NodeInfo @from)
            {
                var operationCount = _node.Config.SuccessorCount;
                var reply = new GetSuccessorTableReply(_node.Identity, @from, operationId)
                {
                    SuccessorTable = new RoutingTableEntry[operationCount]
                };

                var opIds = GetOperationIds(operationCount);
                var multiReplyHandler = _node.CreateAwaitAllResponsesHandler();
                multiReplyHandler.PerformAction(() =>
                {
                    // send a single message which clones as it travels
                })
                .AndAwaitAll(opIds, (GetSuccessorReply successorReply) =>
                {
                    reply.SuccessorTable[successorReply.SuccessorIndex] =
                        new RoutingTableEntry(successorReply.Successor.RoutingHash, successorReply.Successor);
                })
                .ContinueWith(() =>
                {
                    //send reply
                })
                .Run(operationId);

                //if (_node.IsInDomain(applicantInfo.RoutingHash))
                //{
                //    // this node is the successor + (r-1) of this nodes successor list.
                //}
                //else // Ask the network
                //{
                //    var closestNode = _node.FingerTable.FindClosestPrecedingFinger(applicantInfo.RoutingHash);
                //    var closestSocket = _node.ForwardingSockets[closestNode.HostAndPort];
                //    _node.Marshaller.Send(msg, closestSocket);
                //}
            }

            public void Handle(GetSuccessor message)
            {

            }

            private void GetRoutingTable(CorrelationId operationId, NodeInfo applicantInfo)
            {
                
            }

        }
    }
}
