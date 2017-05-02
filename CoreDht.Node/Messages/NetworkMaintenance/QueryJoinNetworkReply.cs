using System;
using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class QueryJoinNetworkReply : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public QueryJoinNetworkReply(NodeInfo @from, NodeInfo to, Guid correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
        public RoutingTableEntry[] RoutingTable { get; set; }
        public RoutingTableEntry[] SuccessorTable { get; set; }
    }
}