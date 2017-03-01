using CoreDht;

namespace Routing
{
    public class StartUsefulWork : RoutableMessage
    {
        public StartUsefulWork(ConsistentHash routingTarget) : base(routingTarget)
        { }

        public string Owner { get; set; }
    }
}