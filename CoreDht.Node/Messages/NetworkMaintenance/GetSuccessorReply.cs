using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetSuccessorReply : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetSuccessorReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public int SuccessorIndex { get; set; }

        public NodeInfo Successor { get; set; }
    }
}