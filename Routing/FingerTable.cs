using CoreDht;

namespace Routing
{
    public class FingerTable
    {
        private readonly FingerTableEntry[] _fingerTableEntries;

        public FingerTable(ChordNodeInfo identity)
        {
            var entryCount = identity.NodeKey.BitCount;
            var seedValue = identity.NodeKey;

            _fingerTableEntries = new FingerTableEntry[entryCount];
            for(int i = 0; i < entryCount; ++i)
            {
                var finger = seedValue + ConsistentHash.BitValue(i);
                _fingerTableEntries[i] = new FingerTableEntry(finger, seedValue);
            }
        }

        public FingerTableEntry this[int index]
        {
            get { return _fingerTableEntries[index]; }
            set { _fingerTableEntries[index] = value; }
        }
    }
}