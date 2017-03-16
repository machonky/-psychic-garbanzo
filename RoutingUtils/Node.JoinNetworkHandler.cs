using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    partial class Node
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
    }
}
