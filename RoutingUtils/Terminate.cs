namespace CoreDht
{
    public class Terminate : RoutableMessage
    {
        public Terminate(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}