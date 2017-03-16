using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// FindSuccessor is transmitted to identify a successor to a node or reconstruct a routing table.
    /// </summary>
    public class FindSuccessor : NodeMessage, ICorrelatedMessage<Guid>
    {
        public NodeInfo ToNode { get; }

        public Guid CorrelationId { get; }

        public FindSuccessor(NodeInfo recipient, NodeInfo toNode, Guid correlationId) : base(recipient)
        {
            ToNode = toNode;
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
            public NodeInfo[] SuccessorList { get; }

            public Reply(NodeInfo sender, NodeInfo successor, Guid correlationId, NodeInfo[] successorList) : base(sender)
            {
                Successor = successor;
                CorrelationId = correlationId;
                SuccessorList = successorList;
            }
        }
    }
}