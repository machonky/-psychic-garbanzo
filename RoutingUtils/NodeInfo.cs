using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;

namespace CoreDht
{
    public class NodeInfo : IComparable<NodeInfo>
    {
        public string Identifier { get; }
        public ConsistentHash RoutingHash { get; }
        public string HostAndPort { get; }

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
                RoutingHash = lhs.RoutingHash;
                HostAndPort = lhs.HostAndPort;
            }
        }

        public override string ToString()
        {
            return Identifier;
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode()^RoutingHash.GetHashCode()^HostAndPort.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as NodeInfo;
            if (rhs != null)
            {
                return
                    Identifier.Equals(rhs.Identifier) &&
                    RoutingHash.Equals(rhs.RoutingHash) &&
                    HostAndPort.Equals(rhs.HostAndPort);
            }
            return false;
        }

        public int CompareTo(NodeInfo other)
        {
            return 
                RoutingHash < other.RoutingHash?-1:
                RoutingHash.Equals(other.RoutingHash)?0:1;
        }
    }
}