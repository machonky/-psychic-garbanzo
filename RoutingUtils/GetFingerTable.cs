using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class GetFingerTable : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public NodeInfo ForNode { get; }

        public GetFingerTable(NodeInfo recipient, NodeInfo forNode, Guid correlationId) : base(recipient)
        {
            ForNode = forNode;
            CorrelationId = correlationId;
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public RoutingTableEntry[] RoutingTableEntries { get; }

            public Reply(NodeInfo sender, Guid correlationId, RoutingTableEntry[] routingTableEntries) : base(sender)
            {
                CorrelationId = correlationId;
                RoutingTableEntries = routingTableEntries;
            }
        }
    }
}