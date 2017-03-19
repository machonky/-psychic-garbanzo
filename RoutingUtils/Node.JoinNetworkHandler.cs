using System;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class JoinNetworkHandler : StateHandler
            , IAmTriggeredBy<JoinNetwork>
            , IAmTriggeredBy<JoinNetwork.Await>
            , IHandle<JoinNetwork.Reply>
        {
            public JoinNetworkHandler(Guid correlationId, Node node) : base(correlationId, node)
            {}

            public void Handle(JoinNetwork message)
            {
                // acquire the necessary information for a node sending this message
                // It may only be correct for a short time if lots of nodes are joining at roughly the same time.

                var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                Node.MessageBus.Subscribe(responder);

                var joinNetworkReply = new JoinNetwork.Reply(message.Recipient, message.CorrelationId);

                responder.Publish(
                    new GetFingerTable(Node.Identity, message.Recipient, GetNextCorrelation()),
                    (GetFingerTable.Reply reply) =>
                    {
                        joinNetworkReply.RoutingTableEntries = reply.RoutingTableEntries;
                        if (joinNetworkReply.IsReadyToTransmit)
                        {
                            CloseHandlerWithReply(joinNetworkReply, joinNetworkReply.Sender, responder);
                        }
                    });

                // TODO: Multiple successors
                //responder.Publish(
                //    new GetSuccessor(Node.Identity, message.Recipient, GetNextCorrelation()),
                //    (GetSuccessor.Reply reply) =>
                //    {
                //        joinNetworkReply.SuccessorList = reply.SuccessorList;
                //        if (joinNetworkReply.IsReadyToTransmit)
                //        {
                //            CloseHandlerWithReply(joinNetworkReply, joinNetworkReply.Sender, responder);
                //        }
                //    });

            }

            public void Handle(JoinNetwork.Await message) {}

            public void Handle(JoinNetwork.Reply message)
            {
                // Now we have the reply we begin the 3 step process to join the overlay network
                // 1. Assign the successor
                Node.Successor = message.RoutingTableEntries[0].SuccessorIdentity;
                // 2.

                // 3. Copy data to the new host
                
            }
        }
    }
}
