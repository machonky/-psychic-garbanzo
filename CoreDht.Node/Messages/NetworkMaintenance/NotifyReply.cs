using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class NotifyReply : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public NotifyReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}