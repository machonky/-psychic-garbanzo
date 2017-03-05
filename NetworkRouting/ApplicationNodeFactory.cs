using CoreDht;
using Routing;

namespace NetworkRouting
{
    public class ApplicationNodeFactory : INodeFactory
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly IMessageSerializer _serializer;
        private readonly INodeSocketFactory _nodeSocketFactory;

        public ApplicationNodeFactory(IConsistentHashingService hashingService, IMessageSerializer serializer, INodeSocketFactory nodeSocketFactory)
        {
            _hashingService = hashingService;
            _serializer = serializer;
            _nodeSocketFactory = nodeSocketFactory;
        }

        public Node CreateNode(string uniqueIdentifier, string hostAndPort)
        {
            var identity = new NodeInfo
            {
                Identifier = uniqueIdentifier,
                RoutingHash = _hashingService.GetConsistentHash(uniqueIdentifier),
                HostAndPort = hostAndPort,
            };
            return new ApplicationNode(identity, _serializer, _nodeSocketFactory, _hashingService);
        }
    }
}