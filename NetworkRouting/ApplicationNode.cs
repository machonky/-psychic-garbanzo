using System;
using CoreDht;
using CoreMemoryBus;
using Routing;

namespace NetworkRouting
{
    public class ApplicationNode : Node
    {
        public ApplicationNode(NodeInfo identity, ApplicationNodeConfiguration config) : 
            base(identity, config)
        {
            MessageBus.Subscribe(new ApplicationHandler(this));
        }

        class ApplicationHandler : IHandle<NodeInitialised>
        {
            private readonly ApplicationNode _host;

            public ApplicationHandler(ApplicationNode host)
            {
                _host = host;
            }

            public void Handle(NodeInitialised message)
            {
                Console.WriteLine($"{_host.Identity.Identifier} initialised");
            }
        }
    }
}
