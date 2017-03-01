using System;
using CoreDht;

namespace Routing.Messages
{
    public class FindSuccessor : RoutableMessage
    {
        public FindSuccessor(ConsistentHash routingTarget, ConsistentHash replyHash, IReplyEnvelope<Result> reply):base(routingTarget)
        {
            ReplyHash = replyHash;
            Send = reply;
        }

        public ConsistentHash SuccessorTo { get; set; }

        public ConsistentHash ReplyHash { get; set; }
        public IReplyEnvelope<Result> Send { get; set; }

        public class Result : RoutableMessage
        {
            public Result(ConsistentHash routingTarget):base(routingTarget)
            { }

            public ChordNodeInfo Successor { get; set; }
        }
    }
}