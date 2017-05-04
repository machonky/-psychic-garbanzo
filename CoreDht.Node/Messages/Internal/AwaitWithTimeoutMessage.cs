using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    public class AwaitWithTimeoutMessage : Message, ICorrelatedMessage<CorrelationId>
    {
        public AwaitWithTimeoutMessage(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public int Timeout { get; set; }
    }
}