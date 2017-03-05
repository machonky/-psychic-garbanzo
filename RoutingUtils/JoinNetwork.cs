using System;

namespace CoreDht
{
    public class JoinNetwork : RoutableMessage, ICorrelatedMessage<Guid>
    {
        public NodeInfo HostIdentity { get; }
        public Guid CorrelationId { get; }

        public JoinNetwork(NodeInfo hostIdentity, ConsistentHash routingTarget, Guid correlationId) : base(routingTarget)
        {
            HostIdentity = hostIdentity;
            CorrelationId = correlationId;
        }

        public class Reply : RoutableMessage, ICorrelatedMessage<Guid>
        {
            public NodeInfo Successor { get; }
            public Guid CorrelationId { get; }

            public Reply(NodeInfo successor, ConsistentHash routingTarget, Guid correlationId) : base(routingTarget)
            {
                Successor = successor;
                CorrelationId = correlationId;
            }
        }
    }
}