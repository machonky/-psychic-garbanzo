using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// FindSuccessor is transmitted to identify a successor to a node or reconstruct a routing table.
    /// </summary>
    public class FindSuccessorToHash : NodeMessage, ICorrelatedMessage<Guid>
    {
        public ConsistentHash ToHash { get; }

        public Guid CorrelationId { get; }

        public FindSuccessorToHash(NodeInfo recipient, ConsistentHash toHash, Guid correlationId) : base(recipient)
        {
            ToHash = toHash;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// This is an internal message to ensure that we can catch a FindSuccessor request that is forwarded over the network
        /// </summary>
        public class Await : AwaitMessage
        {
            public Await(Guid correlationId):base(correlationId)
            { }
        }

        /// <summary>
        /// When a successor to the query above can be identified the reply is sent to facilitate a Join request or rebuilding a routing table.
        /// </summary>
        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public NodeInfo Successor { get; }

            public Reply(NodeInfo sender, NodeInfo successor, Guid correlationId) : base(sender)
            {
                Successor = successor;
                CorrelationId = correlationId;
            }
        }
    }
}