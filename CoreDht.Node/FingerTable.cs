using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class FingerTable : RoutingTable
    {
        public NodeInfo Identity { get; }

        public FingerTable(NodeInfo identity, int tableLength) : base(tableLength)
        {
            Identity = identity;
            var routingHash = identity.RoutingHash;

            for (int i = 0; i < tableLength; ++i)
            {
                var finger = routingHash + routingHash.One() << i;
                Entries[i] = new RoutingTableEntry(finger, identity);
            }
        }

        public static RoutingTableEntry[] CreateEntries(int entryCount, ConsistentHash nodeHash)
        {
            var entries = RoutingTable.CreateEntries(entryCount);
            for (int i = 0; i < entryCount; ++i)
            {
                var finger = nodeHash + nodeHash.One() << i;
                entries[i] = new RoutingTableEntry(finger, null);
            }

            return entries;
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