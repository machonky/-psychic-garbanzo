using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
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

        public FindSuccessorToHash(NodeInfo identity, ConsistentHash toHash, Guid correlationId) : base(identity)
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

            public Reply(NodeInfo identity, NodeInfo successor, Guid correlationId) : base(identity)
            {
                Successor = successor;
                CorrelationId = correlationId;
            }
        }
    }
}