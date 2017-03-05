using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// This is an internal message to ensure that this node catches the response from the Joinee.
    /// </summary>
    public class AwaitingJoin : Message, ICorrelatedMessage<Guid>
    {
        public AwaitingJoin(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}