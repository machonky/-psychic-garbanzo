using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetSuccessor : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetSuccessor(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public int SuccessorIndex { get; set; }
        public NodeInfo Applicant { get; set; }
        public int HopCount { get; set; }
    }
}