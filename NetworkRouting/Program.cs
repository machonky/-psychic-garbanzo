using System;
using System.Threading;
using CoreDht;
using Routing;
using RoutingUtils;

namespace NetworkRouting
{

    class Program
    {
        void Run(string[] args)
        {
            var hostEntry = _dnsProvider.GetHostEntry("localhost");
            var config = new MyAppNodeConfiguration
            {
                HashingService = _hashingService,
                Serializer = _msgSerializer,
                NodeSocketFactory = _nodeSocketFactory,
                Clock = _clock,
                CorrelationFactory = _correlationFactory,
                SuccessorTableLength = 3,
                SeedNode = $"{hostEntry.HostName}:9000"
            };

            var factory = new MyAppNodeFactory(config);

            var id0 = Node.CreateIdentifier($"{hostEntry.HostName}:9000");
            var id1 = Node.CreateIdentifier($"{hostEntry.HostName}:9001");

            using (var node0 = factory.CreateNode(id0, $"{hostEntry.HostName}:9000"))
            using (var node1 = factory.CreateNode(id1, $"{hostEntry.HostName}:9001"))
            {
                Console.ReadKey();

                var terminate = new TerminateNode(node0.Identity.RoutingHash);
                node0.Publish(terminate);

                terminate = new TerminateNode(node1.Identity.RoutingHash);
                node1.Publish(terminate);
            }
        }

        private readonly IConsistentHashingService _hashingService;
        private readonly IMessageSerializer _msgSerializer;
        private readonly IDnsProvider _dnsProvider;
        private readonly INodeSocketFactory _nodeSocketFactory;
        private readonly IClock _clock;
        private readonly ICorrelationFactory<Guid> _correlationFactory;

        Program()
        {
            _clock = new Clock();
            _dnsProvider = new DnsProvider();
            _hashingService = new Md5HashingService();
            _msgSerializer = new MessageSerializer();
            _nodeSocketFactory = new InProcNodeSocketFactory();
            _correlationFactory = new GuidCorrelationFactory();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}

