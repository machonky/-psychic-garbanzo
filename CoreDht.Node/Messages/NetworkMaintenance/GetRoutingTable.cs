using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetRoutingTable : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetRoutingTable(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public NodeInfo Applicant { get; set; }
    }
}