using System;
using CoreDht;
using CoreMemoryBus;
using Routing;

namespace NetworkRouting
{
    public class MyAppNode : Node
    {
        public MyAppNode(NodeInfo identity, MyAppNodeConfiguration config) : 
            base(identity, config)
        {
            MessageBus.Subscribe(new ApplicationHandler(this));
        }

        class ApplicationHandler : IHandle<NodeInitialised>
        {
            private readonly MyAppNode _host;

            public ApplicationHandler(MyAppNode host)
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
