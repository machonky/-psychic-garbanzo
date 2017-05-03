using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using NetworkRouting;
using Routing;

namespace CoreDht
{
    public class NodeConfiguration
    {
        public IConsistentHashingService HashingService { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public INodeSocketFactory NodeSocketFactory { get; set; }
        public IUtcClock Clock { get; set; }
        public ICorrelationFactory<Guid> CorrelationFactory { get; set; }

        public int SuccessorCount { get; set; }
        public string SeedNode { get; set; } // To be replaced with a multicast beacon
        public Action<string> LoggerDelegate { get; set; }
    }
}