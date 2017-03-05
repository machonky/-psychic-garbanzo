namespace CoreDht
{
    public class NodeReady: RoutableMessage
    {
        public NodeReady(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}