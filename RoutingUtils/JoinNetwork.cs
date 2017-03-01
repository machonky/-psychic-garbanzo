namespace CoreDht
{
    public class JoinNetwork : RoutableMessage
    {
        public JoinNetwork(string hostAddress, ConsistentHash routingTarget) : base(routingTarget)
        {
            HostAddress = hostAddress;
        }

        public string HostAddress { get; private set; }
    }

    public class JoinNetworkReply : RoutableMessage
    {
        public JoinNetworkReply(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}