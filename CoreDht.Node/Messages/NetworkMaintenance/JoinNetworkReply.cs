using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    public class JoinNetworkReply : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public JoinNetworkReply(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
        public RoutingTableEntry[] RoutingTable { get; set; }
        public RoutingTableEntry[] SuccessorTable { get; set; }
    }
}