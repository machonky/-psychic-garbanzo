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

        public class Await : Message, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }

            public Await(Guid correlationId)
            {
                CorrelationId = correlationId;
            }
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public FingerTableEntry[] FingerTableEntries { get; }

            public Reply(NodeInfo sender, Guid correlationId, FingerTableEntry[] fingerTableEntries) : base(sender)
            {
                CorrelationId = correlationId;
                FingerTableEntries = fingerTableEntries;
            }
        }
    }
}