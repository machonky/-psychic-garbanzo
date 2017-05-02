using System;
using System.Collections.Generic;
using System.Linq;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class AwaitAllResponsesHandler 
        : NodeAwaitHandler
        , IHandle<Message>
        , IHandle<CancelOperation>
        , IHandle<OperationComplete>
    {
        private Action _initAction;
        private Action _continuation;
        private CorrelationId _parentCorrelation;
        private readonly Dictionary<CorrelationId, IResponseAction> _responseActions = new Dictionary<CorrelationId, IResponseAction>();

        public AwaitAllResponsesHandler(IPublisher publisher, ISubscriber subscriber, Action<string> logger) 
            : base(publisher, subscriber, logger)
        {}

        public AwaitAllResponsesHandler PerformAction(Action initAction)
        {
            _initAction = initAction;
            return this;
        }

        public AwaitAllResponsesHandler AndAwait<TResponse>(CorrelationId correlationId, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
            Logger?.Invoke($"Awaiting {typeof(TResponse).Name} Id:{correlationId}");

            return this;
        }

        public AwaitAllResponsesHandler AndAwaitAll<TResponse>(CorrelationId[] correlationIds, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            foreach (var correlationId in correlationIds)
            {
                _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
            }

            var ids = from id in correlationIds select $"{id}";
            Logger?.Invoke($"Awaiting All {typeof(TResponse).Name}\n\tId:{string.Join("\n\tId:", ids)}");

            return this;
        }

        public AwaitAllResponsesHandler ContinueWith(Action continuation)
        {
            _continuation = continuation;
            return this;
        }

        public void Run(CorrelationId parentCorrelation)
        {
            _parentCorrelation = parentCorrelation;
            Publisher.Publish(new AwaitMessage(parentCorrelation));

            Subscriber.Subscribe(this);

            _initAction?.Invoke();
        }

        public void Handle(Message message)
        {
            if (message.GetType() != typeof(CancelOperation) ||
                message.GetType() != typeof(OperationComplete))
            {
                var correlatedMessage = message as ICorrelatedMessage<CorrelationId>;
                if (correlatedMessage != null)
                {
                    IResponseAction response;
                    if (_responseActions.TryGetValue(correlatedMessage.CorrelationId, out response) && response.TryExecuteAction(message))
                    {
                        _responseActions.Remove(correlatedMessage.CorrelationId);
                        if (_responseActions.Count == 0)
                        {
                            _continuation?.Invoke();
                            Publisher.Publish(new OperationComplete(_parentCorrelation));
                        }
                    }
                }
            }
        }

        public void Handle(CancelOperation message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                Subscriber.Unsubscribe(this);

                var operationIds = from opIds in _responseActions.Keys select $"{opIds}";
                Logger?.Invoke($"Cancel Operations\n\tId:{string.Join("\n\tId:",operationIds)}");
            }
        }

        public void Handle(OperationComplete message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                Subscriber.Unsubscribe(this);

                Logger?.Invoke($"Operation Complete Id:{message.CorrelationId}");
            }
        }
    }
}