using CoreMemoryBus.Messages;

namespace NetworkRouting
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        Message Deserialize(string json);
    }
}