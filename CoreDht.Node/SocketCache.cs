using System;
using System.Collections.Generic;
using CoreDht.Utils;

namespace CoreDht.Node
{
    public class SocketCache : ObjectCache<string, DealerSocketWrapper>, IDisposable
    {
        public SocketCache(INodeSocketFactory socketFactory, IClock clock) : 
            base(key => new DealerSocketWrapper(socketFactory.CreateForwardingSocket(key), clock))
        {}

        public override void Remove(string key)
        {
            DealerSocketWrapper socket;
            if (Cache.TryGetValue(key, out socket))
            {
                socket.Dispose();
                base.Remove(key);
            }
        }

        public IEnumerable<string> PurgeFaultySockets()
        {
            var result = new List<string>();
            foreach (var wrapper in Cache)
            {
                if (wrapper.Value.Error)
                {
                    result.Add(wrapper.Key);
                }
            }
            result.ForEach(Remove);
            return result;
        }

        #region IDisposable Support

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                foreach (var socket in Cache.Values)
                {
                    socket.Dispose();    
                }

                isDisposed = true;
            }
        }

        #endregion
    }
}