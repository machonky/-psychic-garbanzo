using CoreMemoryBus.Messages;

namespace CoreDht
{
    public interface IPublisher<in T> where T : Message
    {
        void Publish(T message);
    }
}