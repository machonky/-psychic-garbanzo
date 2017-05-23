using System;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public interface ICommunicationManagerFactory
    {
        ICommunicationManager Create(NodeInfo nodeInfo, INodeMarshaller marshaller, ISocketCache socketCache, IPublisher publisher, Action<string> logger);
    }
}