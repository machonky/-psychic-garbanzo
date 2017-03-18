﻿using System;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class FindSuccessorToHashHandler: StateHandler,
            IAmTriggeredBy<FindSuccessorToHash>,
            IAmTriggeredBy<FindSuccessorToHash.Await>
        {
            public FindSuccessorToHashHandler(Guid correlationId, Node node) : base(correlationId, node)
            {}

            public void Handle(FindSuccessorToHash message)
            {
                if (Node.IsInDomain(message.ToHash))
                {
                    var msg = new FindSuccessorToHash.Reply(message.Recipient, Node.Identity, message.CorrelationId);
                    Node.SendReply(message.Recipient, msg);
                    Node.CloseHandler(CorrelationId);
                }
                else
                {
                    var newCorrelation = Node.CorrelationFactory.GetNextCorrelation();
                    var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                    Node.MessageBus.Subscribe(responder);
                    responder.Publish(
                        new FindSuccessorToHash.Await(newCorrelation),
                        (FindSuccessorToHash.Reply networkReply) =>
                        {
                                // Forward the reply to the original sender with the initial correlation.
                                var msg = new FindSuccessorToHash.Reply(message.Recipient, networkReply.Successor, message.CorrelationId);
                            Node.SendReply(message.Recipient, msg);
                            Node.MessageBus.Unsubscribe(responder);
                            Node.CloseHandler(CorrelationId);
                        });

                    // Forward a new query to another node and reply back to this node.
                    var startValue = message.ToHash;
                    var forwardMsg = new FindSuccessorToHash(Node.Identity, startValue, newCorrelation);
                    var closestNode = Node.FindClosestPrecedingFinger(startValue);
                    var forwardingSocket = Node.ForwardingSockets[closestNode.HostAndPort];
                    Node.Marshaller.Send(forwardMsg, forwardingSocket);
                }
            }

            public void Handle(FindSuccessorToHash.Await message)
            {
                // Just wait for the response from the network
            }
        }
    }
}
