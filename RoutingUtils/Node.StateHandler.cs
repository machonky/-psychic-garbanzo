using System;
using System.Collections.Generic;
using System.Linq;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class StateHandler : RepositoryItem<Guid, StateHandler>,
            IAmTriggeredBy<JoinNetwork>,
            IAmTriggeredBy<JoinNetwork.Await>,
            IAmTriggeredBy<FindSuccessor>,
            IHandle<JoinNetwork.Reply>
        {
            private Node Node { get; }

            public StateHandler(Guid correlationId, CoreDht.Node node) : base(correlationId)
            {
                Node = node;
            }

            public void Handle(JoinNetwork message)
            {
                // This node is being queried to join the network
                // We need to find the successor to the applicant
                var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                Node.MessageBus.Subscribe(responder);

                var newCorrelation = Node.CorrelationFactory.GetNextCorrelation();
                responder.Publish<FindSuccessor, FindSuccessor.Reply>(
                    new FindSuccessor(Node.Identity, message.Recipient, newCorrelation),
                    reply =>
                    {
                        // Then transmit the JoinNetwork.Reply to the applicant
                        var joinReply = new JoinNetwork.Reply(reply.Successor, message.CorrelationId, reply.SuccessorList);
                        var forwardingSocket = Node.ForwardingSockets[message.Recipient.HostAndPort];
                        Node.Marshaller.Send(joinReply, forwardingSocket);

                        Node.MessageBus.Unsubscribe(responder);
                        SendLocalMessage(new OperationComplete(message.CorrelationId));
                    });
            }

            public void Handle(JoinNetwork.Await message)
            {
                // This node is applying to join the network and waiting for a response
            }

            public void Handle(JoinNetwork.Reply message)
            {
                // The network has calculated a successor to connect to.
                Node.Successor = message.Sender;

                // Recalculate the finger table based on this news
                var successors = message.SuccessorList;
                // Initiate the 3 step join process from here
                // #Initialise fingers
                // #Update fingers of existing nodes
                // #Transfer keys

                CloseHandler(message);
            }

            protected void SendLocalMessage(Message msg)
            {
                Node.MessageBus.Publish(msg);
            }

            public void Handle(FindSuccessor message)
            {
                if (Node.IsInDomain(message.ToNode.RoutingHash))
                {
                    // This node is the successor
                    var reply = new FindSuccessor.Reply(
                        message.Recipient,
                        Node.Identity,
                        message.CorrelationId,
                        Node.SuccessorTable.Entries.DistinctNodes());

                    SendReply(message.Recipient, reply);

                    CloseHandler(message);
                }
                else // forward around the ring and wait for a response
                {
                    // We first need to ensure we catch the response here
                    var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                    Node.MessageBus.Subscribe(responder);

                    var @await = new FindSuccessor.Await(message.CorrelationId);
                    responder.Publish<FindSuccessor.Await, FindSuccessor.Reply>(@await,
                        reply =>
                        {
                            SendReply(message.Recipient, new FindSuccessor.Reply(message.Recipient, reply.Successor,
                                message.CorrelationId, reply.SuccessorList));

                            Node.MessageBus.Unsubscribe(responder);
                            CloseHandler(message);
                        });

                    NodeInfo closestNode = Node.FindClosestPrecedingFinger(message.ToNode.RoutingHash);
                    var forwardingSocket = Node.ForwardingSockets[closestNode.HostAndPort];
                    Node.Marshaller.Send(message, forwardingSocket);
                }
            }

            protected void CloseHandler(ICorrelatedMessage<Guid> message)
            {
                SendLocalMessage(new OperationComplete(message.CorrelationId));
            }

            protected void SendReply(NodeInfo target, NodeReply reply)
            {
                if (target.Equals(Node.Identity))
                {
                    Node.MessageBus.Publish(reply);
                }
                else
                {
                    var forwardingSocket = Node.ForwardingSockets[target.HostAndPort];
                    Node.Marshaller.Send(reply, forwardingSocket);
                }
            }
        }
    }
}
