using System.Collections.Generic;
using System.Linq;

namespace CoreDht
{
    static class FingerTableEntryExtensions
    {
        public static NodeInfo[] DistinctNodes(this IEnumerable<FingerTableEntry> entries)
        {
            var result = new SortedSet<NodeInfo>();
            foreach (var entry in entries)
            {
                result.Add(entry.SuccessorIdentity);
            }
            return result.ToArray();
        }
    }
}