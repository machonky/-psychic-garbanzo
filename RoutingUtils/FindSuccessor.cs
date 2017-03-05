using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class FindSuccessor : RoutableMessage, ICorrelatedMessage<Guid>
    {
        public ConsistentHash ToNode { get; }

        public Guid CorrelationId { get; }

        public FindSuccessor(ConsistentHash toNode, Guid correlationId, ConsistentHash routingTarget) : base(routingTarget)
        {
            ToNode = toNode;
            CorrelationId = correlationId;
        }

        public class Reply : RoutableMessage, ICorrelatedMessage<Guid>
        {
            public NodeInfo Successor { get; }
            public Guid CorrelationId { get; }

            public Reply(NodeInfo successor, Guid correlationId, ConsistentHash routingTarget) : base(routingTarget)
            {
                Successor = successor;
                CorrelationId = correlationId;
            }
        }
    }
}