using System;
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
            var id0 = Node.CreateIdentifier(hostEntry.HostName, 9000, 0);
            var id1 = Node.CreateIdentifier(hostEntry.HostName, 9001, 1);

            using (var node0 = _nodeFactory.CreateNode(id0, $"{hostEntry.HostName}:9000"))
            using (var node1 = _nodeFactory.CreateNode(id1, $"{hostEntry.HostName}:9001"))
            {
                // Node1 wants to join node0 network
                var msg = node1.EmitJoinNetwork(hostEntry.HostName, 9001);
                node0.Publish(msg);
//                var msg = new FindSuccessor(node1.Identity.RoutingHash, Guid.NewGuid(), node0.Identity.RoutingHash);
//                node0.Publish(msg);

                var terminate = new TerminateNode(node0.Identity.RoutingHash);
                node0.Publish(terminate);

                terminate = new TerminateNode(node1.Identity.RoutingHash);
                node1.Publish(terminate);
            }
        }

        private readonly IConsistentHashingService _hashingService;
        private readonly INodeFactory _nodeFactory;
        private readonly IMessageSerializer _msgSerializer;
        private readonly IDnsProvider _dnsProvider;
        private readonly INodeSocketFactory _nodeSocketFactory;

        Program()
        {
            _hashingService = new Md5HashingService();
            _msgSerializer = new MessageSerializer();
            _nodeSocketFactory = new InProcNodeSocketFactory();
            _nodeFactory = new ApplicationNodeFactory(_hashingService, _msgSerializer, _nodeSocketFactory);
            _dnsProvider = new DnsProvider();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}

