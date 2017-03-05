using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    ///  Internal message to signal to remove the StateHandler
    /// </summary>
    public class OperationComplete : Message, ICorrelatedMessage<Guid>
    {
        public OperationComplete(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}