using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class GetFingerTable : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public NodeInfo ForNode { get; }

        public GetFingerTable(NodeInfo identity, NodeInfo forNode, Guid correlationId) : base(identity)
        {
            ForNode = forNode;
            CorrelationId = correlationId;
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public RoutingTableEntry[] RoutingTableEntries { get; }

            public Reply(NodeInfo identity, Guid correlationId, RoutingTableEntry[] routingTableEntries) : base(identity)
            {
                CorrelationId = correlationId;
                RoutingTableEntries = routingTableEntries;
            }
        }
    }
}