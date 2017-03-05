using Routing;

namespace CoreDht
{
    public class NodeInfo : ICloneable<NodeInfo>
    {
        public string Identifier { get; set; }
        public ConsistentHash RoutingHash { get; set; }
        public string HostAndPort { get; set; }

        public NodeInfo()
        {}

        public NodeInfo(string identifier, ConsistentHash routingHash, string hostAndPort)
        {
            Identifier = identifier;
            RoutingHash = routingHash;
            HostAndPort = hostAndPort;
        }

        private NodeInfo(NodeInfo lhs)
        {
            if (lhs != null)
            {
                Identifier = lhs.Identifier;
                RoutingHash = lhs.RoutingHash.Clone();
                HostAndPort = lhs.HostAndPort;
            }
        }

        public NodeInfo Clone()
        {
            return new NodeInfo(this);
        }
    }
}