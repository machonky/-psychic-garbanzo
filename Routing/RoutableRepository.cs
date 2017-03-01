using System;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace Routing
{
    public class RoutableRepository<THashKey, TRepoItem> : Repository<THashKey, TRepoItem>, IPublisher<RoutableMessage> where TRepoItem : IPublisher
    {
        protected RoutableRepository(Func<Message, TRepoItem> repoItemFactory = null) : base(repoItemFactory)
        { }

        public bool TryPublish(RoutableMessage msg)
        {
            var repoItemMessage = msg as IRoutableMessage<THashKey>;
            if (repoItemMessage != null)
            {
                var routingHash = repoItemMessage.RoutingTarget;
                TRepoItem repoItem;
                if (RepoItems.TryGetValue(routingHash, out repoItem))
                {
                    repoItem.Publish(msg);
                    return true;
                }

                if (TriggerMessageTypes.Contains(msg.GetType()))
                {
                    var newRepoItem = RepoItemFactory(msg);
                    RepoItems[routingHash] = newRepoItem;
                    newRepoItem.Publish(msg);
                    return true;
                }
            }

            return false;
        }
    }
}