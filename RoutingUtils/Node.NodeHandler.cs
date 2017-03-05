using System;
using CoreMemoryBus;

namespace CoreDht
{
    public partial class Node
    {
        public class NodeHandler : CorrelatableRepository<Guid, NodeHandler.StateHandler>,
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

            public void Handle(CancelOperation message)
            {
                Remove(message.CorrelationId);
            }

            public enum QueryState
            {
                None,
                Join,
                AwaitJoin,
                FindSuccessor,
                AwaitFindSuccessor
            }

            public class StateHandler : RepositoryItem<Guid, StateHandler>,
                IAmTriggeredBy<FindSuccessor>,
                IHandle<FindSuccessor.Reply>, 
                IAmTriggeredBy<JoinNetwork>,
                IHandle<JoinNetwork.Reply>
            {
                private NodeInfo _applicantIdentity;
                Node Node { get; }

                private QueryState State { get; set; }

                public StateHandler(Guid correlationId, Node node) : base(correlationId)
                {
                    Node = node;
                }

                public void Handle(FindSuccessor message)
                {
                    if (State == QueryState.None) State = QueryState.FindSuccessor;

                    if (Node.IsInDomain(message.ToNode))
                    {
                        Handle(new FindSuccessor.Reply(Node.Identity, message.CorrelationId, message.RoutingTarget));
                        Node.MessageBus.Publish(new OperationComplete(message.CorrelationId));
                    }
                    else
                    {
                        // Find a new node to route the request to
                        var nearestResult = Node.FindClosestPrecedingFinger(message.ToNode);
                        // Get or create a new socket from the cache
                        var forwardingSocket = Node.ForwardingSockets[nearestResult.HostAndPort];
                        //... and try again
                        Node.Marshaller.Send(new FindSuccessor(message.ToNode, Guid.NewGuid(), Node.Identity.RoutingHash), forwardingSocket);
                    }
                }

                public void Handle(FindSuccessor.Reply message)
                {
                    if (State == QueryState.FindSuccessor)
                    {
                        // Dunno yet
                    }
                    else if (State == QueryState.Join)
                    {
                        // Transmit a join reply with details from message - this is always a socket message
                        //Handle(new JoinNetwork.Reply(message.Successor, message.RoutingTarget, message.CorrelationId));
                        SendReply(new JoinNetwork.Reply(message.Successor, message.RoutingTarget, message.CorrelationId), null);
                    }
                }

                void SendReply(RoutableMessage message, string hostAndPort)
                {
                    if (message.RoutingTarget.Equals(Node.Identity.RoutingHash))
                    {
                        Node.MessageBus.Publish(message);
                    }
                    else
                    {
                        var forwardingSocket = Node.ForwardingSockets[hostAndPort];
                        Node.Marshaller.Send(message, forwardingSocket);
                    }
                }

                public void Handle(JoinNetwork message)
                {
                    if (State == QueryState.None) State = QueryState.Join;

                    _applicantIdentity = message.HostIdentity;
                    // Find the successor to the applicant
                    Node.MessageBus.Publish(new FindSuccessor(message.HostIdentity.RoutingHash, message.CorrelationId, Node.Identity.RoutingHash));
                }

                public void Handle(JoinNetwork.Reply message)
                {
                    if (State == QueryState.Join)
                    {
                        // Assign the node successor and acquire the predecessor

                        // We're complete. Remove this state handler
                        Node.MessageBus.Publish(new OperationComplete(message.CorrelationId));
                    }
                }
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
        }
    }
}