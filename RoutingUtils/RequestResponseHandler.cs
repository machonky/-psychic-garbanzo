using System;
using System.Collections.Generic;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    //public class RequestResponseHandler<TCorrelation> : IHandle<Message>
    //{
    //    private readonly Dictionary<TCorrelation, IResponseAction> _responseActions = new Dictionary<TCorrelation, IResponseAction>();
    //    private readonly IPublisher _publisher;

    //    public RequestResponseHandler(IPublisher publisher)
    //    {
    //        this._publisher = publisher;
    //    }

    //    public void Publish<TRequest, TResponse>(TRequest request, Action<TResponse> responseCallback) 
    //        where TRequest : Message, ICorrelatedMessage<TCorrelation> 
    //        where TResponse : Message, ICorrelatedMessage<TCorrelation>
    //    {
    //        _responseActions[request.CorrelationId] = new ResponseAction<TResponse>(responseCallback);
    //        _publisher.Publish(request);
    //    }

    //    public void Handle(Message message)
    //    {
    //        var correlatedMessage = message as ICorrelatedMessage<TCorrelation>;
    //        IResponseAction responseAction;
    //        if (correlatedMessage == null || !_responseActions.TryGetValue(correlatedMessage.CorrelationId, out responseAction))
    //        {
    //            return;
    //        }

    //        responseAction.ExecuteAction(message);
    //        _responseActions.Remove(correlatedMessage.CorrelationId);
    //    }

    //    private interface IResponseAction
    //    {
    //        void ExecuteAction(Message replyMessage);
    //    }

    //    private class ResponseAction<TMessage> : IResponseAction where TMessage : Message
    //    {
    //        private readonly Action<TMessage> _action;

    //        public ResponseAction(Action<TMessage> action)
    //        {
    //            _action = action;
    //        }

    //        public void ExecuteAction(Message replyMessage)
    //        {
    //            _action((TMessage)replyMessage);
    //        }
    //    }
    //}
}