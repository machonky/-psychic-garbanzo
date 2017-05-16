using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class Rectify : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public Rectify(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public NodeInfo Predecessor { get; set; }
    }
}