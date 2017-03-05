using CoreDht;
using Routing;

namespace NetworkRouting
{
    public class ApplicationNode : Node
    {
        public ApplicationNode(NodeInfo identity, IMessageSerializer serializer, INodeSocketFactory socketFactory, IConsistentHashingService hashingService) : 
            base(identity, serializer, socketFactory, hashingService)
        {
            MessageBus.Subscribe(new ApplicationHandler(this));
        }

        class ApplicationHandler
        {
            private readonly ApplicationNode _host;

            public ApplicationHandler(ApplicationNode host)
            {
                _host = host;
            }
        }
    }
}
