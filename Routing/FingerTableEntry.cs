using CoreDht;

namespace Routing
{
    public class FingerTableEntry
    {
        public FingerTableEntry(ConsistentHash startValue, ConsistentHash successorId)
        {
            StartValue = startValue;
            SuccessorId = successorId;
        }

        public ConsistentHash StartValue { get; private set; }
        public ConsistentHash SuccessorId { get; private set; }
    }
}