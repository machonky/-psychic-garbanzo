using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetSuccessorTable : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetSuccessorTable(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public NodeInfo Applicant { get; set; }

        public RoutingTableEntry[] SuccessorTable { get; set; }

        public int HopCount { get; set; }
    }
}