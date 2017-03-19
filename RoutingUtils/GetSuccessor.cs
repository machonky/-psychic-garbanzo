using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class GetSuccessor : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public int SuccessorIndex { get; set; }
        public NodeInfo ForNode { get; }

        public GetSuccessor(NodeInfo recipient, NodeInfo forNode, Guid correlationId, int successorIndex) : base(recipient)
        {
            ForNode = forNode;
            CorrelationId = correlationId;
            SuccessorIndex = successorIndex;
        }

        public class Await : AwaitMessage
        {
            public Await(Guid correlationId) : base(correlationId)
            { }
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public NodeInfo Successor { get; }
            public int SuccessorIndex { get; }

            public Reply(NodeInfo sender, Guid correlationId, NodeInfo successor, int successorIndex) : base(sender)
            {
                CorrelationId = correlationId;
                SuccessorIndex = successorIndex;
                Successor = successor;
            }
        }
    }
}