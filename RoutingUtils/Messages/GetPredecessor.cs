using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// GetPredecessor queries a node of it's predecessor. The Identity in the message is the identity of the reply recipient
    /// </summary>
    public class GetPredecessor : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }

        public GetPredecessor(NodeInfo identity, Guid correlationId) : base(identity)
        {
            CorrelationId = correlationId;
        }

        public class Await : AwaitMessage
        {
            public Await(Guid correlationId) : base(correlationId)
            {}
        }

        /// <summary>
        /// The Identity supplied in the Reply is the identity of the predecessor
        /// </summary>
        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Reply(NodeInfo identity, Guid correlationId, NodeInfo successor) : base(identity)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; }
        }

    }
}