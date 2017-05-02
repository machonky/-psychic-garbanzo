using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messages;

namespace CoreDht.Utils.Messages
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