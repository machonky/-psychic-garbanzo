using CoreMemoryBus.Messages;

namespace Routing
{
    public interface IPublisher<in T> where T : Message
    {
        bool TryPublish(T message);
    }
}