using CoreDht;
using Routing;

namespace NetworkRouting
{
    public class ApplicationNodeFactory : INodeFactory
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly IMessageSerializer _serializer;

        public ApplicationNodeFactory(IConsistentHashingService hashingService, IMessageSerializer serializer)
        {
            _hashingService = hashingService;
            _serializer = serializer;
        }

        public Node CreateNode(string uniqueIdentifier)
        {
            var identity = new NodeInfo {Identifier = uniqueIdentifier, RoutingHash = _hashingService.GetConsistentHash(uniqueIdentifier)};
            return new ApplicationNode(identity, _serializer);
        }
    }
}