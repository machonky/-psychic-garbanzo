namespace CoreDht.Utils.Hashing
{
    public interface IConsistentHashingService
    {
        ConsistentHash GetConsistentHash(string key);
    }
}