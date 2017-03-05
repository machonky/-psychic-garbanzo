using CoreMemoryBus.Messaging;

namespace CoreDht
{
    public interface IRepositoryItem<THashKey> : ICorrelatableItem<THashKey>, IPublisher
    { }
}