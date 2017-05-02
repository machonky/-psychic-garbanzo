using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class RoutingTableEntry
    {
        public RoutingTableEntry(ConsistentHash startValue, NodeInfo successorIdentity)
        {
            StartValue = startValue;
            SuccessorIdentity = successorIdentity;
        }

        public ConsistentHash StartValue { get; }
        public NodeInfo SuccessorIdentity { get; }
    }
}