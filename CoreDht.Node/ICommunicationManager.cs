using CoreDht.Node.Messages;
using CoreDht.Utils;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public interface ICommunicationManager
    {
        void SendInternal(Message msg);
        void Send(PointToPointMessage msg);
        void SendAck(PointToPointMessage originatingMessage, CorrelationId correlation);
        void Receive(NetMQMessage msg);
    }
}
