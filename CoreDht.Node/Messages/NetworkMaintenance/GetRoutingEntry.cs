using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetRoutingEntry : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetRoutingEntry(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public ConsistentHash StartValue { get; set; }
    }
}