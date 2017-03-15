namespace CoreDht
{
    public class RoutingTable
    {
        protected RoutingTable(int tableLength)
        {
            Entries = CreateEntries(tableLength);
        }

        public static FingerTableEntry[] CreateEntries(int tableLength)
        {
            return new FingerTableEntry[tableLength];
        }

        public FingerTableEntry[] Entries { get; }

        public FingerTableEntry this[int index]
        {
            get { return Entries[index]; }
            set { Entries[index] = value; }
        }

        public void Copy(RoutingTable other)
        {
            if (other != null)
            {
                for (int i = 0; i < other.Entries.Length; ++i)
                {
                    Entries[i] = other.Entries[i];
                }
            }
        }

        public void Copy(FingerTableEntry[] fingerTableEntries)
        {
            for (int i = 0; i < fingerTableEntries.Length; ++i)
            {
                Entries[i] = fingerTableEntries[i];
            }
        }
    }
}