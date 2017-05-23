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
        public IUtcClock Clock { get; set; }
        public ICorrelationFactory<CorrelationId> CorrelationFactory { get; set; }
        public IRandomNumberGenerator Random { get; set; }
        public IActionTimerFactory ActionTimerFactory { get; set; }
        public IExpiryTimeCalculator ExpiryCalculator { get; set; }
        public INodeMarshallerFactory MarshallerFactory { get; set; }
        public ICommunicationManagerFactory CommunicationManagerFactory { get; set; }

        public int SuccessorCount { get; set; }
        public string SeedNode { get; set; }
        public Action<string> LoggerDelegate { get; set; }
        public JoinConfiguration JoinSettings { get; }
        public AwaitConfiguration AwaitSettings { get; }
        public StabilizeConfiguration StabilizeSettings { get; }

        public NodeConfiguration()
        {
            JoinSettings = new JoinConfiguration();
            AwaitSettings = new AwaitConfiguration();
            StabilizeSettings = new StabilizeConfiguration();

            SuccessorCount = 3;
        }
    }

    public class StabilizeConfiguration
    {
        public StabilizeConfiguration()
        {
            StabilizeMinInterval = 120000;
            StabilizeMaxInterval = 180000;
        }

        public int StabilizeMinInterval { get; set; }
        public int StabilizeMaxInterval { get; set; }
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