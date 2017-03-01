using CoreDht;

namespace NetworkRouting
{
    public class ApplicationNode : Node
    {
        public ApplicationNode(NodeInfo identity, IMessageSerializer serializer) : base(identity, serializer)
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
