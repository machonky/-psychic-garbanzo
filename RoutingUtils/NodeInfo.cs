using Routing;

namespace CoreDht
{
    public class NodeInfo : ICloneable<NodeInfo>
    {
        public string Identifier { get; set; }
        public ConsistentHash RoutingHash { get; set; }
        public NodeInfo Clone()
        {
            return new NodeInfo
            {
                Identifier = Identifier,
                RoutingHash = RoutingHash,
            };
        }
    }
}