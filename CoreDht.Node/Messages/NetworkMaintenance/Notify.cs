using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class Notify : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public Notify(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
        public NodeInfo NewSuccessor { get; set; }
    }
}