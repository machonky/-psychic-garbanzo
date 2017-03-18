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
    }
}
