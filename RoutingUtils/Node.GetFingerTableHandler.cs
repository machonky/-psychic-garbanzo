using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class GetFingerTableHandler :
            CorrelatableRepository<Guid, GetFingerTableHandler.StateHandler>,
            IHandle<CancelOperation>,
            IHandle<OperationComplete>
        {
            protected Node Node { get; }

            public GetFingerTableHandler(Node node)
            {
                Node = node;
                RepoItemFactory = CreateStateHandler;
            }

            public class StateHandler :
                RepositoryItem<Guid, StateHandler>,
                IAmTriggeredBy<GetFingerTable>,
                IAmTriggeredBy<GetFingerTable.Await>,
                IHandle<GetFingerTable.Reply>
            {
                public Node Node { get; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }

                public void Handle(GetFingerTable message)
                {
                    var bitCount = message.Recipient.RoutingHash.BitCount;
                    var entries = FingerTable.CreateEntries(bitCount, message.ForNode.RoutingHash);
                    var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                    Node.MessageBus.Subscribe(responder);
                    var replyCount = new Reference<int>(0); // ensure all replies share the current count
                    for (int i = 0; i < bitCount; ++i)
                    {
                        var newCorrelation = Node.CorrelationFactory.GetNextCorrelation();
                        var index = i; // Value must be constant in the scope of the reply
                        responder.Publish(
                            new FindSuccessorToHash(Node.Identity, entries[i].StartValue, newCorrelation),
                            (FindSuccessorToHash.Reply reply) =>
                            {
                                entries[index] = new FingerTableEntry(entries[index].StartValue, reply.Successor);
                                ++replyCount.Value;
                                if (replyCount == bitCount)
                                {
                                    Node.MessageBus.Unsubscribe(responder);
                                    Node.SendReply(message.Recipient, new GetFingerTable.Reply(Node.Identity, message.CorrelationId, entries));
                                    Node.CloseHandler(CorrelationId);
                                }
                            });
                    }
                }

                public void Handle(GetFingerTable.Await message)
                {
                    // Create this handler to await a response
                }

                public void Handle(GetFingerTable.Reply message)
                {
                    // Integrate the finger table data
                    Node.FingerTable.Copy(message.FingerTableEntries); // we'll make this part of a 'Fat message' later.
                }
            }

            StateHandler CreateStateHandler(Message msg)
            {
                return new StateHandler(((ICorrelatedMessage<Guid>)msg).CorrelationId, Node);
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
