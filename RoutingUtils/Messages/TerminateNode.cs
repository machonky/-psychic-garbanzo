using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class TerminateNode : RoutableMessage
    {
        public TerminateNode(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}