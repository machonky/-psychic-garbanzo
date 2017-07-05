namespace CoreDht.Node
{
    public class RoutingTable
    {
        protected RoutingTable(int tableLength)
        {
            Entries = CreateEntries(tableLength);
        }

        public static RoutingTableEntry[] CreateEntries(int tableLength)
        {
            return new RoutingTableEntry[tableLength];
        }

        public RoutingTableEntry[] Entries { get; }

        public int Length => Entries.Length;

        public RoutingTableEntry this[int index]
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

        public void Copy(RoutingTableEntry[] routingTableEntries)
        {
            for (int i = 0; i < routingTableEntries.Length; ++i)
            {
                Entries[i] = routingTableEntries[i];
            }
        }
    }
}