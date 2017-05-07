using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class GetRoutingEntryReply : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public GetRoutingEntryReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }

        public RoutingTableEntry Entry { get; set; }
    }
}