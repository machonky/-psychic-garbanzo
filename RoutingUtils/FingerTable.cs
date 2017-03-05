using NetMQ;

namespace CoreDht
{
    public class FingerTable
    {
        public NodeInfo Identity { get; set; }

        private readonly FingerTableEntry[] _fingerTableEntries;

        public FingerTable(NodeInfo identity, IOutgoingSocket nodeSocket)
        {
            Identity = identity;
            var routingHash = identity.RoutingHash;
            var entryCount = routingHash.BitCount;

            _fingerTableEntries = new FingerTableEntry[entryCount];
            for (int i = 0; i < entryCount; ++i)
            {
                var finger = routingHash + ConsistentHash.BitValue(i);
                _fingerTableEntries[i] = new FingerTableEntry(finger, identity);
            }
        }

        public FingerTableEntry this[int index]
        {
            get { return _fingerTableEntries[index]; }
            set { _fingerTableEntries[index] = value; }
        }

        public NodeInfo FindClosestPrecedingFinger(ConsistentHash toNode)
        {
            //Check finger tables
            for (int i = _fingerTableEntries.Length - 1; i >= 0; --i)
            {
                if (Node.IsIDInDomain(_fingerTableEntries[i].SuccessorIdentity.RoutingHash, Identity.RoutingHash, toNode))
                {
                    return _fingerTableEntries[i].SuccessorIdentity;
                }
            }
            // Check successors

            return Identity;
        }
    }
}