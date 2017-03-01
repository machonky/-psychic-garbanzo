using CoreMemoryBus.Messages;

namespace Routing.Messages
{
    public interface IReplyEnvelope<TReply> where TReply:Message
    {
        void Reply(TReply replyMessage);
    }
}