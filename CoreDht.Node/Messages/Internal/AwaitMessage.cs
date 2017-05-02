using System;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    public class AwaitMessage : Message, ICorrelatedMessage<Guid>
    {
        public AwaitMessage(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
}