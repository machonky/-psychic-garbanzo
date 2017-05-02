using System;
using System.Collections.Generic;

namespace CoreDht.Utils
{
    public class ObjectCache<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _factory;
        protected Dictionary<TKey, TValue> Cache { get; }

        public ObjectCache(Func<TKey, TValue> factory)
        {
            _factory = factory;
            Cache = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!Cache.TryGetValue(key, out value))
                {
                    value = _factory(key);
                    Cache[key] = value;
                }
                return value;
            }
        }

        public virtual void Remove(TKey key)
        {
            Cache.Remove(key);
        }
    }
}