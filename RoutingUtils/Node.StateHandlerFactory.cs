using System;
using System.Collections.Generic;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Util;

namespace CoreDht
{
    partial class Node
    {
        public class StateHandlerFactory
        {
            private static readonly Dictionary<Type, Func<Guid, Node, StateHandler>> _stateHandlers =
                new Dictionary<Type, Func<Guid, Node, StateHandler>>();

            static StateHandlerFactory()
            {
                var handlerTypes = new[]
                {
                    typeof(JoinNetworkHandler),
                    typeof(FindSuccessorToHashHandler),
                    typeof(GetFingerTableHandler),
                    typeof(GetSuccessorHandler),
                    typeof(RequestKeysHandler)
                };

                foreach (var handlerType in handlerTypes)
                {
                    var triggerHandlerInterfaces = PubSubCommon.GetMessageTriggerInterfaces(handlerType.GetInterfaces());
                    Func<Guid, Node, StateHandler> createHandler =
                        (guid, node) => (StateHandler) Activator.CreateInstance(handlerType, guid, node);

                    foreach (var triggers in triggerHandlerInterfaces)
                    {
                        var msgType = triggers.GetGenericArguments()[0];
                        _stateHandlers.Add(msgType, createHandler);
                    }
                }
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
