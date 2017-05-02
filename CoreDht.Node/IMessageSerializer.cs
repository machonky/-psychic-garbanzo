using CoreMemoryBus.Messages;

namespace CoreDht.Node
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        Message Deserialize(string json);
    }

    public interface IMessageSerializer<T>
    {
        T Serialize(Message message);
        Message Deserialize(T msgData);
    }
}