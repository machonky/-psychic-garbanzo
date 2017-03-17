using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class FindSuccessorToHashHandler :
            CorrelatableRepository<Guid, FindSuccessorToHashHandler.StateHandler>,
            IHandle<CancelOperation>,
            IHandle<OperationComplete>
        {
            protected Node Node { get; }

            public FindSuccessorToHashHandler(Node node)
            {
                Node = node;
                RepoItemFactory = CreateStateHandler;
            }

            StateHandler CreateStateHandler(Message msg)
            {
                return new StateHandler(((ICorrelatedMessage<Guid>)msg).CorrelationId, Node);
            }

            public class StateHandler :
                RepositoryItem<Guid, StateHandler>,
                IAmTriggeredBy<FindSuccessorToHash>,
                IAmTriggeredBy<FindSuccessorToHash.Await>
            {
                public Node Node { get; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }

                public void Handle(FindSuccessorToHash message)
                {
                    if (Node.IsInDomain(message.ToHash))
                    {
                        var msg = new FindSuccessorToHash.Reply(message.Recipient, Node.Identity, message.CorrelationId);
                        Node.SendReply(message.Recipient, msg);
                        Node.CloseHandler(CorrelationId);
                    }
                    else
                    {
                        var newCorrelation = Node.CorrelationFactory.GetNextCorrelation();
                        var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                        Node.MessageBus.Subscribe(responder);
                        responder.Publish(
                            new FindSuccessorToHash.Await(newCorrelation),
                            (FindSuccessorToHash.Reply networkReply) =>
                            {
                                // Forward the reply to the original sender with the initial correlation.
                                var msg = new FindSuccessorToHash.Reply(message.Recipient, networkReply.Successor, message.CorrelationId);
                                Node.SendReply(message.Recipient, msg);
                                Node.MessageBus.Unsubscribe(responder);
                                Node.CloseHandler(CorrelationId);
                            });

                        // Forward a new query to another node and reply back to this node.
                        var startValue = message.ToHash;
                        var forwardMsg = new FindSuccessorToHash(Node.Identity, startValue, newCorrelation);
                        var closestNode = Node.FindClosestPrecedingFinger(startValue);
                        var forwardingSocket = Node.ForwardingSockets[closestNode.HostAndPort];
                        Node.Marshaller.Send(forwardMsg, forwardingSocket);
                    }
                }

                public void Handle(FindSuccessorToHash.Await message)
                {
                    // Just wait for the response from the network
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
