using CoreMemoryBus;
using System;

namespace CoreDht
{
    partial class Node
    {
        public class RequestKeysHandler : StateHandler
            , IAmTriggeredBy<RequestKeys>
            , IAmTriggeredBy<RequestKeys.Await>
            , IHandle<RequestKeys.Reply>
            , IAmTriggeredBy<RequestKeys.AwaitReceipt>
            , IHandle<RequestKeys.Receipt>
        {
            public RequestKeysHandler(Guid correlationId, Node node) : base(correlationId, node)
            { }

            public void Handle(RequestKeys message)
            {
                // A recipient of this message is to create a reply 
            }

            public void Handle(RequestKeys.Await message)
            {
                // A sender of a RequestKeys message is awaits the reply
            }

            public void Handle(RequestKeys.Reply message)
            {
                // The recipient of a RequestKeys.Reply will acquire all keys related to the range specified.
                // Will need to ensure that it's not possible for the previous keys owner after sending to process 
                // new messages for those keys. They will o course require routing to the new owner. As long as the routing table
                // is adjusted before the transfer this will not be a problem. So the transfer must trigger a routing table refresh 
                // before accepting new app messages.
                // The reply contains a Receipt correlation which allows a safe transfer of keys from node to node.
                // The recipient will send a receipt to the previous owner which will then delete those keys on confirmation.
            }

            public void Handle(RequestKeys.AwaitReceipt message)
            {
            }

            public void Handle(RequestKeys.Receipt message)
            {
                // On receipt of this message the previous owner of keys can confirm that the keys have reached theor destination and can delete the local copies.
            }
        }
    }
}
