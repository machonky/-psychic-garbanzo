using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

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
                , IAmTriggeredBy<JoinNetwork>
                , IAmTriggeredBy<JoinNetwork.Await>
                , IHandle<JoinNetwork.Reply>
            {
                public Node Node { get; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }

                public void Handle(JoinNetwork message)
                {
                    // acquire the necessary information for a node sending this message
                    //  Find the finger table
                    //  Get the successor list 
                    // then send a JoinNetwork.Reply

                    var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                    Node.MessageBus.Subscribe(responder);

                    var newCorrelation = Node.CorrelationFactory.GetNextCorrelation();
                    responder.Publish(
                        new GetFingerTable(Node.Identity, message.Recipient, newCorrelation),
                        (GetFingerTable.Reply reply) =>
                        {

                            Node.CloseHandler(CorrelationId);
                        });
                }

                public void Handle(JoinNetwork.Await message)
                {
                    // Just awaiting the network response to join the network
                }

                public void Handle(JoinNetwork.Reply message)
                {
                    // Now we have caught the reply we begin the 3 step process to join the overlay network
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
