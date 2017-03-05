using System;

namespace CoreDht
{
    /// <summary>
    /// Cancels the operation in progress. If a correlated reply arrives it will be ignored, as there will be no correlated state
    /// </summary>
    public class CancelOperation : RoutableMessage, ICorrelatedMessage<Guid>
    {
        public CancelOperation(ConsistentHash routingTarget, Guid correlationId) : base(routingTarget)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}