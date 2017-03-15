using System;
using NetworkRouting;
using Routing;

namespace CoreDht
{
    public class NodeConfiguration
    {
        public IConsistentHashingService HashingService { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public INodeSocketFactory NodeSocketFactory { get; set; }
        public IClock Clock { get; set; }
        public ICorrelationFactory<Guid> CorrelationFactory { get; set; }
        public int SuccessorTableLength { get; set; }
    }
}