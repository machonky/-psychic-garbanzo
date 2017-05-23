using System;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class CommunicationManagerFactory : ICommunicationManagerFactory
    {
        public ICommunicationManager Create(NodeInfo nodeInfo, INodeMarshaller marshaller, ISocketCache socketCache, IPublisher publisher, Action<string> logger)
        {
            return new CommunicationManager(nodeInfo, marshaller, socketCache, publisher, logger);
        }
    }
}