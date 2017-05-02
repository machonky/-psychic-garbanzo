using System;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class NodeAwaitHandler
    {
        protected readonly IPublisher Publisher;
        protected readonly ISubscriber Subscriber;
        protected readonly Action<string> Logger;

        protected NodeAwaitHandler(IPublisher publisher, ISubscriber subscriber, Action<string> logger)
        {
            Publisher = publisher;
            Subscriber = subscriber;
            Logger = logger;
        }

        protected interface IResponseAction
        {
            bool TryExecuteAction(Message replyMessage);
        }

        protected class ResponseAction<TMessage> : IResponseAction where TMessage : Message
        {
            private readonly Action<TMessage> _action;

            public ResponseAction(Action<TMessage> action)
            {
                _action = action;
            }

            public bool TryExecuteAction(Message replyMessage)
            {
                var reply = replyMessage as TMessage;
                if (reply != null)
                {
                    _action(reply);
                    return true;
                }
                return false;
            }
        }
    }
}