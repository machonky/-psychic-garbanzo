using CoreDht;

namespace Routing.Messages
{
    public class JoinNetwork : RoutableMessage
    {
        public JoinNetwork(ConsistentHash routingTarget) : base(routingTarget)
        { }

        public ChordNodeInfo ApplicantInfo { get; set; }
    }
}