using System;
using System.Collections.Generic;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Util;

namespace CoreDht
{
    /// <summary>
    /// A Repository is a type of dictionary that creates an object corresponding to a key if it does not already exist. 
    /// Objects in this repository must implement an IAmTriggeredBy interface to be instantiated.
    /// <br>
    /// This is particularly useful in messaging applications if the value type can respond to co-related messages (implements IPublisher). 
    /// The correlation value is the Repository key type. A repository can instantiate a handler of the message stream on demand.
    /// </summary>
    /// <typeparam name="THashKey"></typeparam>
    /// <typeparam name="TRepoItem"></typeparam>
    public class Repository<THashKey, TRepoItem>
    {
        protected readonly Dictionary<THashKey, TRepoItem> RepoItems = new Dictionary<THashKey, TRepoItem>();

        protected static readonly HashSet<Type> TriggerMessageTypes = new HashSet<Type>();

        static Repository()
        {
            var triggerHandlers = PubSubCommon.GetMessageTriggerInterfaces(typeof(TRepoItem).GetInterfaces());
            foreach (var trigger in triggerHandlers)
            {
                var triggerMsgType = trigger.GetGenericArguments()[0];
                TriggerMessageTypes.Add(triggerMsgType);
            }
        }

        protected Repository(Func<Message, TRepoItem> repoItemFactory = null)
        {
            RepoItemFactory = repoItemFactory ?? (_ => (TRepoItem)Activator.CreateInstance(typeof(TRepoItem)));
        }

        protected Func<Message, TRepoItem> RepoItemFactory { get; set; }

        public TRepoItem Remove(THashKey key)
        {
            TRepoItem item;
            if (RepoItems.TryGetValue(key, out item))
            {
                RepoItems.Remove(key);
                return item;
            }

            return default(TRepoItem);
        }

        public IEnumerable<KeyValuePair<THashKey, TRepoItem>> Remove(ISet<THashKey> keys)
        {
            var result = new List<KeyValuePair<THashKey, TRepoItem>>();
            foreach (var hashKey in keys)
            {
                TRepoItem item;
                if (RepoItems.TryGetValue(hashKey, out item))
                {
                    result.Add(new KeyValuePair<THashKey, TRepoItem>(hashKey, item));
                }
            }

            return result;
        }
    }
}