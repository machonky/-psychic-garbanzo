using System;
using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.NetworkMaintenance
{
    /// <summary>
    /// An applicant node will send this message to a seed node when initiating a join procedure.
    /// </summary>
    public class QueryJoinNetwork : NodeMessage, ICorrelatedMessage<CorrelationId>
    {
        public QueryJoinNetwork(NodeInfo @from, NodeInfo to, CorrelationId correlationId):base(@from, to)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}