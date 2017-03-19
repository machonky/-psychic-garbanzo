using System;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

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

            protected void SendReplyTo(NodeInfo target, NodeReply reply)
            {
                if (target.Equals(Node.Identity))
                {
                    SendLocalMessage(reply);
                }
                else
                {
                    var forwardingSocket = Node.ForwardingSockets[target.HostAndPort];
                    Node.Marshaller.Send(reply, forwardingSocket);
                }
            }

            protected void ForwardMessageTo(NodeInfo target, NodeMessage msg)
            {
                if (target.Equals(Node.Identity))
                {
                    SendLocalMessage(msg);
                }
                else
                {
                    var forwardingSocket = Node.ForwardingSockets[target.HostAndPort];
                    Node.Marshaller.Send(msg, forwardingSocket);
                }
            }

            protected Guid GetNextCorrelation()
            {
                return Node.CorrelationFactory.GetNextCorrelation();
            }

            protected void CloseHandler()
            {
                Node.CloseHandler(CorrelationId);
            }

            protected void Subscribe(object messageHandler)
            {
                Node.MessageBus.Subscribe(messageHandler);
            }

            protected void Unsubscribe(RequestResponseHandler<Guid> responder)
            {
                Node.MessageBus.Unsubscribe(responder);
            }

            protected void CloseHandlerWithReply(NodeReply replyMsg, NodeInfo replyTarget, RequestResponseHandler<Guid> responder = null)
            {
                SendReplyTo(replyTarget, replyMsg);
                if (responder != null) Unsubscribe(responder);
                CloseHandler();
            }
        }
    }
}
