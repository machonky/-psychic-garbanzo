using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;

namespace CoreDht
{
    public class NodeReady: RoutableMessage
    {
        public NodeReady(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}