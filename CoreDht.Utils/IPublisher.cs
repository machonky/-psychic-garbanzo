using CoreMemoryBus.Messages;

namespace CoreDht.Utils
{
    public interface IPublisher<in T> where T : Message
    {
        void Publish(T message);
    }
}