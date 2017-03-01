using CoreMemoryBus.Messages;

namespace CoreDht
{
    public class RoutableMessage : Message, IRoutableMessage<ConsistentHash>
    {
        protected RoutableMessage(ConsistentHash routingTarget)
        {
            RoutingTarget = routingTarget;
        }

        public ConsistentHash RoutingTarget { get; private set; }
    }
}