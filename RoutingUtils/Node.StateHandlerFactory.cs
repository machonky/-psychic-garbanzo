using System;
using System.Collections.Generic;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    partial class Node
    {
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
                        {typeof(GetSuccessorTable), getSuccessorHandler},
                        {typeof(GetSuccessorTable.Await), getSuccessorHandler},
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
                    Func<Guid, Node, StateHandler> createHandlerFunc;
                    if (_stateHandlers.TryGetValue(msg.GetType(), out createHandlerFunc))
                    {
                        return createHandlerFunc(cMsg.CorrelationId, Node);
                    }
                }

                throw new NotImplementedException($"No state handler for {msg.GetType()}");
            }
        }
    }
}
