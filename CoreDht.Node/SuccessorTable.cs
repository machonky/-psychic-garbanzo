using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class SuccessorTable : RoutingTable
    {
        public NodeInfo Identity { get; set; }

        public SuccessorTable(NodeInfo identity, int successorCount) : base(successorCount)
        {
            Identity = identity;

            for (int i = 0; i < Entries.Length; ++i)
            {
                var finger = identity.RoutingHash;
                Entries[i] = new RoutingTableEntry(finger, identity);
            }
        }

        public NodeInfo FindClosestPrecedingFinger(ConsistentHash toNode)
        {
            //Check finger tables
            for (int i = Entries.Length - 1; i >= 0; --i)
            {
                if (Node.IsIdInDomain(Entries[i].SuccessorIdentity.RoutingHash, Identity.RoutingHash, toNode))
                {
                    return Entries[i].SuccessorIdentity;
                }
            }
            // Check successors

            return Identity;
        }
    }
}