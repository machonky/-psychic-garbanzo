using System;
using CoreDht;

namespace Routing
{
    public class ChordNodeInfo : ICloneable<ChordNodeInfo>
    {
        public string Identifier { get; private set; }
        public ConsistentHash NodeKey { get; private set; }

        public ChordNodeInfo(string identifier, ConsistentHash nodeKey)
        {
            Identifier = identifier;
            NodeKey = nodeKey;
        }

        private ChordNodeInfo(ChordNodeInfo rhs)
        {
            if (rhs != null)
            {
                Identifier = rhs.Identifier;
                NodeKey = rhs.NodeKey;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Identifier, NodeKey);
        }

        public ChordNodeInfo Clone()
        {
            return new ChordNodeInfo(this);
        }
    }
}