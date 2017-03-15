using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public partial class Node
    {
        public class JoinNetworkHandler : 
            CorrelatableRepository<Guid, JoinNetworkHandler.StateHandler>,
            IHandle<CancelOperation>,
            IHandle<OperationComplete>
        {
            protected Node Node { get; }
            
            public JoinNetworkHandler(Node node)
            {
                Node = node;
                RepoItemFactory = CreateStateHandler;
            }

            StateHandler CreateStateHandler(Message msg)
            {
                return new StateHandler(((ICorrelatedMessage<Guid>)msg).CorrelationId, Node);
            }

            public class StateHandler : RepositoryItem<Guid, StateHandler>
            {
                public Node Node { get; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }
            }

            public void Handle(CancelOperation message)
            {
                Remove(message.CorrelationId);
            }

            public void Handle(OperationComplete message)
            {
                Remove(message.CorrelationId);
            }
        }


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
                RepoItemFactory = CreateStateHandler;
            }

            StateHandler CreateStateHandler(Message msg)
            {
                return new StateHandler(((ICorrelatedMessage<Guid>)msg).CorrelationId, Node);
            }

            public void Handle(NodeReady message)
            {
                if (!Node.Identity.HostAndPort.Equals(CreateHostAndPort("Touchy",9000)))
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