using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    /// <summary>
    /// A stabilize message is sent to a seed node after joining to learn of it's new successors predecessor, to adopt the predecessor as it's own.
    /// In a subsequent rectify operation the seed node will adopt the newly joined node as it's new predecessor
    /// </summary>
    public class Stabilize : PointToPointMessage, ICorrelatedMessage<CorrelationId>
    {
        public Stabilize(NodeInfo @from, NodeInfo to, CorrelationId correlationId) : base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}