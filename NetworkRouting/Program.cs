using System;
using System.Net;
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
            var identifier = Node.CreateIdentifier(hostEntry.HostName, 9999);
            Console.WriteLine($"Creating Node \"{identifier}\"");
            using (var newNode = _nodeFactory.CreateNode(identifier))
            {
                var join = new JoinNetwork("Hi", _hashingService.GetConsistentHash("Hi"));
                newNode.Publish(join);

                var terminate = new TerminateNode(newNode.Identity.RoutingHash);
                newNode.Publish(terminate);
            }
        }

        private readonly IConsistentHashingService _hashingService;
        private readonly INodeFactory _nodeFactory;
        private readonly IMessageSerializer _msgSerializer;
        private readonly IDnsProvider _dnsProvider;

        Program()
        {
            _hashingService = new Md5HashingService();
            _msgSerializer = new MessageSerializer();
            _nodeFactory = new ApplicationNodeFactory(_hashingService, _msgSerializer);
            _dnsProvider = new DnsProvider();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}

