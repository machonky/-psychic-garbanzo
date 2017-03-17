using System;
using CoreDht;
using Routing;

namespace NetworkRouting
{
    public class MyAppNodeFactory : INodeFactory
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly MyAppNodeConfiguration _config;

        public MyAppNodeFactory(MyAppNodeConfiguration config)
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

            return new MyAppNode(identity, _config);
        }
    }
}