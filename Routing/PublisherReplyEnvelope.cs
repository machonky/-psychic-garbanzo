using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace Routing.Messages
{
    public class PublisherReplyEnvelope<TReply> : IReplyEnvelope<TReply> where TReply : Message
    {
        public IPublisher Publisher { get; private set; }

        public PublisherReplyEnvelope(IPublisher publisher)
        {
            Publisher = publisher;
        }

        public void Reply(TReply replyMessage)
        {
            Publisher.Publish(replyMessage);
        }
    }
}