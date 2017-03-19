using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class RequestKeys : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }
        public ConsistentHash StartAt { get; }

        public ConsistentHash SuccessorId { get; }

        public RequestKeys(NodeInfo recipient, Guid correlationId, ConsistentHash startAt, ConsistentHash successorId) : base(recipient)
        {
            CorrelationId = correlationId;
            StartAt = startAt;
            SuccessorId = successorId;
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

            public Reply(NodeInfo sender, Guid correlationId, Guid receiptCorrelation) : base(sender)
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
            public Receipt(NodeInfo sender, Guid correlationId) : base(sender)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; }
        }
    }
}