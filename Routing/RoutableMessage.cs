using CoreDht;
using CoreMemoryBus.Messages;

namespace Routing
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