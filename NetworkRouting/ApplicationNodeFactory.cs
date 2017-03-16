using System;
using CoreDht;
using Routing;

namespace NetworkRouting
{
    public class ApplicationNodeFactory : INodeFactory
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly ApplicationNodeConfiguration _config;

        public ApplicationNodeFactory(ApplicationNodeConfiguration config)
        {
            _hashingService = config.HashingService;
            _config = config;
        }

        public Node CreateNode(string uniqueIdentifier, string hostAndPort)
        {
            var identity = new NodeInfo(
                identifier: uniqueIdentifier,
                routingHash: _hashingService.GetConsistentHash(uniqueIdentifier), 
                hostAndPort: hostAndPort);

            return new ApplicationNode(identity, _config);
        }
    }
}