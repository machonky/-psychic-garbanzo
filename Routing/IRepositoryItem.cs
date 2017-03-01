using CoreMemoryBus.Messaging;

namespace Routing
{
    public interface IRepositoryItem<THashKey> : ICorrelatable<THashKey>, IPublisher
    { }
}