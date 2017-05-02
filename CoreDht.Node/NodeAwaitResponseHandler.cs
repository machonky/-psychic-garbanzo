using System;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class NodeAwaitResponseHandler 
        : NodeAwaitHandler
        , IHandle<Message>
        , IHandle<CancelOperation>
        , IHandle<OperationComplete>
    {
        private CorrelationId _correlationId;
        private IResponseAction _responseAction;

        public NodeAwaitResponseHandler(IPublisher publisher, ISubscriber subscriber, Action<string> logger):base(publisher,subscriber,logger)
        {}
        
        public void Await<TResponse>(CorrelationId correlationId, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            _correlationId = correlationId;
            Publisher.Publish(new AwaitMessage(_correlationId));

            Subscriber.Subscribe(this);

            _responseAction = new ResponseAction<TResponse>(responseCallback);
            Logger?.Invoke($"Awaiting response for Id:{_correlationId}");
        }

        public void Handle(Message message)
        {
            if (message.GetType() != typeof(CancelOperation)|| 
                message.GetType() != typeof(OperationComplete))
            {
                var correlatedMessage = message as ICorrelatedMessage<CorrelationId>;
                if (correlatedMessage != null)
                {
                    if (_responseAction.TryExecuteAction(message))
                    {
                        Publisher.Publish(new OperationComplete(correlatedMessage.CorrelationId));
                    }
                }
            }
        }

        public void Handle(CancelOperation message)
        {
            if (message.CorrelationId.Equals(_correlationId))
            {
                Subscriber.Unsubscribe(this);
                Logger?.Invoke($"Cancel Operation {message.CorrelationId}");
            }
        }

        public void Handle(OperationComplete message)
        {
            if (message.CorrelationId.Equals(_correlationId))
            {
                Subscriber.Unsubscribe(this);
                Logger?.Invoke($"Operation Complete {message.CorrelationId}");
            }
        }
    }
}