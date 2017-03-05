using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class TerminateNode : RoutableMessage
    {
        public TerminateNode(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}