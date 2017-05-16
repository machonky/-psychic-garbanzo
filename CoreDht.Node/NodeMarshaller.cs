using CoreDht.Node.Messages;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public class NodeMarshaller : INodeMarshaller
    {
        public const string InternalMessage = "IM";
        public const string PointToPointMessage = "PP";

        private const int PayloadFrameIndex = 1;

        private readonly IMessageSerializer _serializer;

        public NodeMarshaller(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public NetMQMessage Marshall(Message msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(InternalMessage),
                new NetMQFrame(json),
            });

            return result;
        }

        public void Unmarshall(NetMQMessage mqMessage, out Message result)
        {
            result = _serializer.Deserialize<Message>(json: mqMessage[PayloadFrameIndex].ConvertToString());
        }

        public void Send(Message msg, IOutgoingSocket actorSocket)
        {
            var mqMsg = Marshall(msg);
            actorSocket.SendMultipartMessage(mqMsg);
        }

        public NetMQMessage Marshall(PointToPointMessage msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(PointToPointMessage),
                new NetMQFrame(json),
            });

            return result;
        }

        public void Unmarshall(NetMQMessage mqMessage, out PointToPointMessage result)
        {
            result = _serializer.Deserialize<PointToPointMessage>(json: mqMessage[PayloadFrameIndex].ConvertToString());
        }

        public void Send(PointToPointMessage msg, IOutgoingSocket forwardingSocket)
        {
            var mqMsg = Marshall(msg);
            forwardingSocket.SendMultipartMessage(mqMsg);
        }
    }
}