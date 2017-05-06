using CoreMemoryBus.Messages;

namespace CoreDht.Node
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        TMessage Deserialize<TMessage>(string json) where TMessage : Message;
    }

    public interface IMessageSerializer<T>
    {
        T Serialize(Message message);
        Message Deserialize(T msgData);
    }
}