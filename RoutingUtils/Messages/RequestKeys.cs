using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class RequestKeys : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public ConsistentHash StartAt { get; }

        public NodeInfo Successor { get; }

        public RequestKeys(NodeInfo identity, Guid correlationId, ConsistentHash startAt, NodeInfo successor) : base(identity)
        {
            CorrelationId = correlationId;
            StartAt = startAt;
            Successor = successor;
        }

        public class Await : AwaitMessage
        {
            public Await(Guid correlationId) : base(correlationId)
            {}
        }

        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }

            public Guid ReceiptCorrelation { get; }

            public Reply(NodeInfo identity, Guid correlationId, Guid receiptCorrelation) : base(identity)
            {
                ReceiptCorrelation = receiptCorrelation;
                CorrelationId = correlationId;
            }
        }

        public class AwaitReceipt : AwaitMessage
        {
            public AwaitReceipt(Guid correlationId) : base(correlationId)
            {}
        }

        public class Receipt : NodeReply, ICorrelatedMessage<Guid>
        {
            public Receipt(NodeInfo identity, Guid correlationId) : base(identity)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; }
        }
    }
}