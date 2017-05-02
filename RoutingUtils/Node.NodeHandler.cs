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

        public class NotifyPredecessorHandler
            : IHandle<NotifyPredecessor>
        {
            public Node Node { get; }

            public NotifyPredecessorHandler(Node node)
            {
                Node = node;
            }

            public void Handle(NotifyPredecessor message)
            {
                var joinee = message.Identity;
                Node.Logger?.Invoke($"{Node.Identity.Identifier} received NotifyPredecessor from {joinee.Identifier}");

                if (Node.Predecessor == null ||
                    Node.IsIDInDomain(joinee.RoutingHash, Node.Predecessor.RoutingHash, Node.Identity.RoutingHash))
                {
                    Node.Logger?.Invoke($"{Node.Identity.Identifier} accepts {joinee.Identifier} as predecessor.");
                    NodeInfo prevPredecessor = Node.Predecessor;
                    Node.Predecessor = joinee;

                    if (prevPredecessor != null)
                    {
                        Node.Logger?.Invoke($"{Node.Identity.Identifier} alerting previous predecessor {prevPredecessor} to stabilize");
                        Node.ForwardMessageTo(prevPredecessor, new Stabilize(Node.Identity));
                    }
                }
            }
        }

        public class StabilizeHandler
            : IHandle<Stabilize>
        {
            public Node Node { get; }

            public StabilizeHandler(Node node)
            {
                Node = node;
            }

            public void Handle(Stabilize message)
            {
                Node.Logger?.Invoke($"{Node.Identity.Identifier} received Stabilize from {message.Identity}");
                // Get the predecessor of my current successor
                var theBus = Node.MessageBus;
                var nextGuid = Node.CorrelationFactory.GetNextCorrelation();
                var responder = new RequestResponseHandler<Guid>(theBus);
                theBus.Subscribe(responder);

                responder.Await(nextGuid,
                    (GetPredecessor.Reply reply) =>
                    {
                        Node.Logger?.Invoke($"GetPredecessor reply from {Node.Successor}");
                        // If I am not the predecessor, the result is my new successor
                        //if (reply.Identity == null)
                        //{
                        //    Node.Predecessor = Node.Successor; // Close the loop
                        //}
                        //else
                        {
                            // If I am not the predecessor, the result is my new successor
                            if (reply.Identity != null && !Node.Identity.Equals(reply.Identity))
                            {
                                if (Node.IsIDInDomain(
                                        reply.Identity.RoutingHash,
                                        Node.Identity.RoutingHash,
                                        Node.Successor.RoutingHash))
                                {
                                    Node.Successor = message.Identity;
                                }
                            }

                            //if (!Node.Identity.Equals(reply.Identity))
                            //{
                            //    Node.Logger?.Invoke($"{Node.Identity} assigning predecessor as {reply.Identity}");
                            //    Node.Predecessor = reply.Identity;
                            //}
                        }
                        // Send notify predecessor to my new successor
                        Node.Logger?.Invoke($"{Node.Identity} sending NotifyPredecessor to {Node.Successor}");
                        Node.ForwardMessageTo(Node.Successor, new NotifyPredecessor(Node.Identity));

                        //if (reply.Identity != null && !Node.Identity.Equals(reply.Identity))
                        //{
                        //    Node.Logger?.Invoke($"{Node.Identity} assigning successor as {reply.Identity}");
                        //    Node.Successor = reply.Identity;

                        //    // Send notify predecessor to my new successor
                        //    Node.Logger?.Invoke($"{Node.Identity} sending NotifyPredecessor to {Node.Successor}");
                        //    Node.ForwardMessageTo(Node.Successor, new NotifyPredecessor(Node.Identity));
                        //}
                        //else
                        //{
                        //    Node.Logger?.Invoke($"{Node.Identity} is stable. S({Node.Successor}) P({Node.Predecessor})");
                        //}
                        theBus.Unsubscribe(responder);
                    });

                // Kick the whole sequence off
                Node.Logger?.Invoke($"Sending GetPredecessor request to {Node.Successor}");
                Node.ForwardMessageTo(Node.Successor, new GetPredecessor(Node.Identity, nextGuid));
            }
        }

        public class GetPredecessorHandler: IHandle<GetPredecessor>
        {
            public Node Node { get; }

            public GetPredecessorHandler(Node node)
            {
                Node = node;
            }

            public void Handle(GetPredecessor message)
            {
                var reply = new GetPredecessor.Reply(Node.Predecessor, message.CorrelationId, Node.Successor);
                Node.SendReplyTo(message.Identity, reply);
            }
        }
    }

    public class RequestResponseHandler<TCorrelation> : IHandle<Message>
    {
        private readonly Dictionary<TCorrelation, IResponseAction> _responseActions = new Dictionary<TCorrelation, IResponseAction>();
        private readonly IPublisher _publisher;

        public RequestResponseHandler(IPublisher publisher)
        {
            this._publisher = publisher;
        }

        public void Publish<TRequest, TResponse>(TRequest request, Action<TResponse> responseCallback)
            where TRequest : Message, ICorrelatedMessage<TCorrelation>
            where TResponse : Message, ICorrelatedMessage<TCorrelation>
        {
            _responseActions[request.CorrelationId] = new ResponseAction<TResponse>(responseCallback);
            _publisher.Publish(request);
        }

        public void Await<TResponse>(TCorrelation correlationId, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<TCorrelation>
        {
            _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
        }


        public void Handle(Message message)
        {
            var correlatedMessage = message as ICorrelatedMessage<TCorrelation>;
            IResponseAction responseAction;
            if (correlatedMessage == null || !_responseActions.TryGetValue(correlatedMessage.CorrelationId, out responseAction))
            {
                return;
            }

            responseAction.ExecuteAction(message);
            _responseActions.Remove(correlatedMessage.CorrelationId);
        }

        private interface IResponseAction
        {
            void ExecuteAction(Message replyMessage);
        }

        private class ResponseAction<TMessage> : IResponseAction where TMessage : Message
        {
            private readonly Action<TMessage> _action;

            public ResponseAction(Action<TMessage> action)
            {
                _action = action;
            }

            public void ExecuteAction(Message replyMessage)
            {
                _action((TMessage)replyMessage);
            }
        }
    }

}