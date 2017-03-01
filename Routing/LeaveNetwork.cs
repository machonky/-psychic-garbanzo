using CoreDht;

namespace Routing.Messages
{
    public class LeaveNetwork : RoutableMessage
    {
        public LeaveNetwork(ConsistentHash routingTarget) : base(routingTarget)
        {}

        public ConsistentHash Departee { get; set; }
    }
}