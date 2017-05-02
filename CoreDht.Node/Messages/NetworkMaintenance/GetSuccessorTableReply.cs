using System;
using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetSuccessorTableReply : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetSuccessorTableReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public RoutingTableEntry[] SuccessorTable { get; set; }
    }
}