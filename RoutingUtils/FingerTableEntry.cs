namespace CoreDht
{
    public class FingerTableEntry
    {
        public FingerTableEntry(ConsistentHash startValue, NodeInfo successorIdentity)
        {
            StartValue = startValue;
            SuccessorIdentity = successorIdentity;
        }

        public ConsistentHash StartValue { get; }
        public NodeInfo SuccessorIdentity { get; }
    }
}