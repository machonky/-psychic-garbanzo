using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class StabilizeReply : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public StabilizeReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public RoutingTableEntry[] SuccessorTableEntries { get; set; }
        public NodeInfo Predecessor { get; set; }
    }
}