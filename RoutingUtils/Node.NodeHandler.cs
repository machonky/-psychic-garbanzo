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
                    // Then transmit the JoinNetwork.Reply
                    State = QueryState.Done;
                    SendLocalMessage(new OperationComplete(message.CorrelationId));
                }

                public void Handle(AwaitingJoin message)
                {
                    // This node is applying to join the network and waiting for a response
                    State = QueryState.AwaitJoin; 
                }

                public void Handle(JoinNetwork.Reply message)
                {
                    // The mesh has calculated the successor to connect to.
                    // Connect up to the successors etc.
                    State = QueryState.Done;
                    SendLocalMessage(new OperationComplete(message.CorrelationId));
                }

                private void SendLocalMessage(Message msg)
                {
                    Node.MessageBus.Publish(msg);
                }
            }
        }
    }
}