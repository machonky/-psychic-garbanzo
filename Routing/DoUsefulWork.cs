using CoreDht;

namespace Routing
{
    public class DoUsefulWork : RoutableMessage
    {
        public DoUsefulWork(ConsistentHash routingTarget) : base(routingTarget)
        { }

        public string Data { get; set; }
    }
}