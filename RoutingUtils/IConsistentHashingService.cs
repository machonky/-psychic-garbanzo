using CoreDht;

namespace Routing
{
    public interface IConsistentHashingService
    {
        ConsistentHash GetConsistentHash(string key);
    }
}