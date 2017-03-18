using System;
using System.Collections.Generic;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using CoreMemoryBus.Util;

namespace CoreDht
{
    partial class Node
    {
        /// <summary>
        /// A StateHandler is a handler created at runtime by a repository in response to a message that will 
        /// cause the handler to be instantiated. Typically for async-awaiting a response to a network message to another node.
        /// The correlationId of the handler is the same as the message that caused it to be instantiated.
        /// </summary>
        public class StateHandler : RepositoryItem<Guid>
        {
            protected Node Node { get; }

            protected StateHandler(Guid correlationId, CoreDht.Node node) : base(correlationId)
            {
                Node = node;
            }

            protected void SendLocalMessage(Message msg)
            {
                Node.MessageBus.Publish(msg);
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

        public class StateHandlerFactory
        {
            private static readonly Dictionary<Type, Func<Guid, Node, StateHandler>> _stateHandlers;

            static StateHandlerFactory()
            {
                Func<Guid, Node, StateHandler>
                    joinNetworkHandler = (guid, node) => new JoinNetworkHandler(guid, node),
                    findSuccessorHandler = (guid, node) => new FindSuccessorToHashHandler(guid, node),
                    getFingerTableHandler = (guid, node) => new GetFingerTableHandler(guid, node),
                    getSuccessorHandler = (guid, node) => new GetSuccessorListHandler(guid, node);

                _stateHandlers =
                    new Dictionary<Type, Func<Guid, Node, StateHandler>>
                    {
                        {typeof(JoinNetwork), joinNetworkHandler},
                        {typeof(JoinNetwork.Await), joinNetworkHandler},
                        {typeof(FindSuccessorToHash), findSuccessorHandler},
                        {typeof(FindSuccessorToHash.Await), findSuccessorHandler},
                        {typeof(GetFingerTable), getFingerTableHandler},
                        {typeof(GetFingerTable.Await), getFingerTableHandler},
                        {typeof(GetSuccessorTable),getSuccessorHandler},
                        {typeof(GetSuccessorTable.Await),getSuccessorHandler},
                    };
            }

            public StateHandlerFactory(Node node)
            {
                Node = node;
            }

            public Node Node { get; }

            public IEnumerable<Type> TriggerMessageTypes => _stateHandlers.Keys;

            public StateHandler CreateHandler(Message msg)
            {
                var cMsg = msg as ICorrelatedMessage<Guid>;
                if (cMsg != null)
                {
                    Func<Guid, Node, StateHandler> factory;
                    if (_stateHandlers.TryGetValue(msg.GetType(), out factory))
                    {
                        return factory(cMsg.CorrelationId, Node);
                    }
                }
                throw new NotImplementedException($"No state handler for {msg.GetType()}");
            }
        }
    }
}
