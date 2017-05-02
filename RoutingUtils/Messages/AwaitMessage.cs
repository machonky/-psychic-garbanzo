using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// An AwaitMessage should always be locally posted to a message bus prior to sending a 
    /// correlated message that expects a reply. This ensures that a handler is instantiated 
    /// to handle the reply.
    /// </summary>
    public class AwaitMessage : Message, ICorrelatedMessage<Guid>
    {
        protected AwaitMessage(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}