using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class GetSuccessorTable : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public NodeInfo ForNode { get; }

        public GetSuccessorTable(NodeInfo recipient, NodeInfo forNode, Guid correlationId) : base(recipient)
        {
            ForNode = forNode;
            CorrelationId = correlationId;
        }

        public class Await : AwaitMessage
        {
            public Await(Guid correlationId) : base(correlationId)
            { }
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }

            public Reply(NodeInfo sender, Guid correlationId) : base(sender)
            {
                CorrelationId = correlationId;
            }
        }
    }
}