namespace CoreDht
{
    public class FingerTable
    {
        private readonly FingerTableEntry[] _fingerTableEntries;

        public FingerTable(NodeInfo identity)
        {
            var routingHash = identity.RoutingHash;
            var entryCount = routingHash.BitCount;

            _fingerTableEntries = new FingerTableEntry[entryCount];
            for (int i = 0; i < entryCount; ++i)
            {
                var finger = routingHash + ConsistentHash.BitValue(i);
                _fingerTableEntries[i] = new FingerTableEntry(finger, routingHash);
            }
        }

        public FingerTableEntry this[int index]
        {
            get { return _fingerTableEntries[index]; }
            set { _fingerTableEntries[index] = value; }
        }
    }
}