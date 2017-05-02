using CoreDht.Node.Messages;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public interface INodeMarshaller : IInternalMessageMarshaller, INodeMessageMarshaller
    {}

    public interface IInternalMessageMarshaller
    {
        NetMQMessage Marshall(Message msg);
        void Unmarshall(NetMQMessage mqMessage, out Message result);
        void Send(Message msg, IOutgoingSocket actorSocket);
    }

    public interface INodeMessageMarshaller
    {
        NetMQMessage Marshall(NodeMessage msg);
        void Unmarshall(NetMQMessage mqMessage, out NodeMessage result);
        void Send(NodeMessage msg, IOutgoingSocket forwardingSocket);
    }
}