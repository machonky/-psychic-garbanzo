using NetMQ;

namespace CoreDht
{
    public class FingerTableEntry
    {
        public FingerTableEntry(ConsistentHash startValue, NodeInfo successorIdentity)
        {
            StartValue = startValue;
            SuccessorIdentity = successorIdentity;
        }

        public ConsistentHash StartValue { get; private set; }
        public NodeInfo SuccessorIdentity { get; private set; }

    }
}