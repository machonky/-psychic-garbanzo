using System;
using System.Collections.Generic;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    public partial class Node
    {
        public class NodeHandler : CorrelatableRepository<Guid, StateHandler>,
            IHandle<NodeReady>,
            IHandle<TerminateNode>,
            IHandle<CancelOperation>,
            IHandle<OperationComplete>
        {
            protected Node Node { get; }
            
            public NodeHandler(Node node)
            {
                Node = node;
                var stateFactory = new StateHandlerFactory(Node);
                foreach (var messageType in stateFactory.TriggerMessageTypes)
                {
                    // Ordinarily the Repository can discover all trigger messages in the RepositoryItem
                    // but since there are multiple types of repo items in this repo, we need to give it 
                    // some assistance. The factory tells the repo what triggers it can handle.
                    TriggerMessageTypes.Add(messageType);
                }
                RepoItemFactory = stateFactory.CreateHandler;
            }

            public void Handle(NodeReady message)
            {
                if (!Node.Identity.HostAndPort.Equals(Node.SeedNode))
                {
                    Node.Go();
                }
                SendLocalMessage(new NodeInitialised());
            }

            public void Handle(CancelOperation message)
            {
                Remove(message.CorrelationId);
            }

            public void Handle(TerminateNode message)
            {
                if (Node.Identity.RoutingHash.Equals(message.RoutingTarget))
                {
                    Node.Poller.Stop();
                }
            }

            public void Handle(OperationComplete message)
            {
                Remove(message.CorrelationId);
            }

            private void SendLocalMessage(Message msg)
            {
                Node.MessageBus.Publish(msg);
            }
        }
    }
}