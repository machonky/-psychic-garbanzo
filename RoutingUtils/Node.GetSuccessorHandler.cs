using System;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class GetSuccessorHandler : StateHandler // TODO
            , IAmTriggeredBy<GetSuccessor>
            , IAmTriggeredBy<GetSuccessor.Await>
        {
            public GetSuccessorHandler(Guid correlationId, Node node) :base(correlationId, node)
            {}

            public void Handle(GetSuccessor message)
            {
                var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                Subscribe(responder);

                var remainingResponses = new Reference<int>(Node.SuccessorCount);



                if (message.SuccessorIndex >= 0)
                {
                    var nextSuccessorIndex = message.SuccessorIndex - 1;

                    // Forward to the next successor
                    var forwardMsg = new GetSuccessor(message.Identity, message.ForNode, GetNextCorrelation(), nextSuccessorIndex);
                    ForwardMessageTo(Node.Successor, forwardMsg); // trap a potential loop
                }

                // reply with this node information
            }

            public void Handle(GetSuccessor.Await message)
            {}
        }
    }
}