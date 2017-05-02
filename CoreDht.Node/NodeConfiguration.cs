using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class NodeConfiguration
    {
        public IConsistentHashingService HashingService { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public INodeSocketFactory NodeSocketFactory { get; set; }
        public IClock Clock { get; set; }
        public ICorrelationFactory<CorrelationId> CorrelationFactory { get; set; }
        public IRandomNumberGenerator Random { get; set; }
        public IActionTimerFactory ActionTimerFactory { get; set; }
        public IExpiryTimeCalculator ExpiryCalculator { get; set; }
        public INodeMarshallerFactory MarshallerFactory { get; set; }

        public int SuccessorCount { get; set; }
        public string SeedNode { get; set; }
        public Action<string> LoggerDelegate { get; set; }
        public JoinConfiguration JoinSettings { get; }
        public int AwaitTimeout { get; set; }
        public AwaitConfiguration AwaitSettings { get; }

        public NodeConfiguration()
        {
            JoinSettings = new JoinConfiguration();
            AwaitSettings = new AwaitConfiguration();
            SuccessorCount = 3;
        }
    }

    public class JoinConfiguration
    {
        public JoinConfiguration()
        {
            JoinMinTimeout = 10;
            JoinMaxTimeout = 500;
        }

        public int JoinMinTimeout { get; set; }
        public int JoinMaxTimeout { get; set; }
    }
}