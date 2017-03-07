using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    public partial class Node
    {
        public class NodeHandler : CorrelatableRepository<Guid, NodeHandler.StateHandler>,
            IHandle<NodeReady>,
            IHandle<TerminateNode>,
            IHandle<CancelOperation>,
            IHandle<OperationComplete>
        {
            private Node Node { get; }

            public NodeHandler(Node node)
            {
                Node = node;
                RepoItemFactory = msg => new StateHandler(((ICorrelatedMessage<Guid>)msg).CorrelationId, Node);
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

            public enum QueryState
            {
                None,
                Join,
                AwaitJoin,
                FindSuccessor,
                AwaitFindSuccessor,
                Done,
            }

            public class StateHandler : RepositoryItem<Guid, StateHandler>,
                IAmTriggeredBy<JoinNetwork>,
                IAmTriggeredBy<AwaitingJoin>,
                IAmTriggeredBy<FindSuccessor>,
                IHandle<JoinNetwork.Reply>
            {
                Node Node { get; }

                private QueryState State { get; set; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }

                public void Handle(JoinNetwork message)
                {
                    State = QueryState.Join; // This node is being queried to join the network
                    // We need to find the successor to the applicant
                    var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                    Node.MessageBus.Subscribe(responder);

                    responder.Publish<FindSuccessor,FindSuccessor.Reply>(
                        new FindSuccessor(message.HostIdentity.RoutingHash, Guid.NewGuid(), Node.Identity.RoutingHash),
                        reply =>
                        {
                            // Then transmit the JoinNetwork.Reply
                            var joinReply = new JoinNetwork.Reply(reply.Successor, reply.RoutingTarget, message.CorrelationId);
                            var forwardingSocket = Node.ForwardingSockets[message.HostIdentity.HostAndPort];
                            Node.Marshaller.Send(joinReply, forwardingSocket);

                            Node.MessageBus.Unsubscribe(responder);
                            State = QueryState.Done;
                            SendLocalMessage(new OperationComplete(message.CorrelationId));
                        });
                }

                public void Handle(AwaitingJoin message)
                {
                    // This node is applying to join the network and waiting for a response
                    State = QueryState.AwaitJoin; 
                }

                public void Handle(JoinNetwork.Reply message)
                {
                    // The mesh has calculated the successor to connect to.
                    Node.Successor = message.Successor;

                    // Recalculate the finger table based on this news

                    // Connect up to the successors etc.
                    State = QueryState.Done;
                    SendLocalMessage(new OperationComplete(message.CorrelationId));
                }

                private void SendLocalMessage(Message msg)
                {
                    Node.MessageBus.Publish(msg);
                }

                public void Handle(FindSuccessor message)
                {
                    State = QueryState.FindSuccessor;
                    if (Node.IsInDomain(message.ToNode))
                    {
                        // This node is the successor
                        SendLocalMessage(new FindSuccessor.Reply(Node.Identity, message.CorrelationId, message.RoutingTarget));
                        State = QueryState.Done;
                        SendLocalMessage(new OperationComplete(message.CorrelationId));
                    }
                    else // forward around the ring
                    {
                        NodeInfo closestNode = Node.FindClosestPrecedingFinger(message.ToNode);

                        var forwardingSocket = Node.ForwardingSockets[closestNode.HostAndPort];
                        Node.Marshaller.Send(message, forwardingSocket);

                        var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                        Node.MessageBus.Subscribe(responder);

                        responder.Publish<FindSuccessor, FindSuccessor.Reply>(message, reply =>
                            {
                            // Then transmit the FindSuccessor.Reply on the reverse path


                                Node.MessageBus.Unsubscribe(responder);
                                State = QueryState.Done;
                                SendLocalMessage(new OperationComplete(message.CorrelationId));
                            });
                    }
                }
            }
        }
    }
}