using System;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;

namespace CoreDht
{
    partial class Node
    {
        public class GetFingerTableHandler: StateHandler,
            IAmTriggeredBy<GetFingerTable>
        {
            public GetFingerTableHandler(Guid correlationId, Node node) : base(correlationId, node)
            {}

            public void Handle(GetFingerTable message)
            {
                var bitCount = message.Identity.RoutingHash.BitCount;
                var entries = FingerTable.CreateEntries(bitCount, message.ForNode.RoutingHash);

                var responder = new RequestResponseHandler<Guid>(Node.MessageBus);
                Node.MessageBus.Subscribe(responder);

                var replyCount = new Reference<int>(0); // ensure all replies share the current count
                for (int i = 0; i < bitCount; ++i)
                {
                    var index = i; // Value must be constant in the scope of the reply
                    responder.Publish(
                        new FindSuccessorToHash(Node.Identity, entries[i].StartValue, GetNextCorrelation()),
                        (FindSuccessorToHash.Reply reply) =>
                        {
                            entries[index] = new RoutingTableEntry(entries[index].StartValue, reply.Successor);
                            ++replyCount.Value;
                            if (replyCount == bitCount)
                            {
                                var getFingerTableReply = new GetFingerTable.Reply(Node.Identity, message.CorrelationId,entries);
                                CloseHandlerWithReply(getFingerTableReply, message.Identity, responder);
                            }
                        });
                }
            }
        }
    }
}
