using CoreDht.Node.Messages;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public class NodeMarshaller : INodeMarshaller
    {
        public const string InternalMessage = "IM";
        public const string NodeMessage = "NM";

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
            result = _serializer.Deserialize(json: mqMessage[PayloadFrameIndex].ConvertToString());
        }

        public void Send(Message msg, IOutgoingSocket actorSocket)
        {
            var mqMsg = Marshall(msg);
            actorSocket.SendMultipartMessage(mqMsg);
        }

        public NetMQMessage Marshall(NodeMessage msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(NodeMessage),
                new NetMQFrame(json),
            });

            return result;
        }

        public void Unmarshall(NetMQMessage mqMessage, out NodeMessage result)
        {
            result = (NodeMessage)_serializer.Deserialize(json: mqMessage[PayloadFrameIndex].ConvertToString());
        }

        public void Send(NodeMessage msg, IOutgoingSocket forwardingSocket)
        {
            var mqMsg = Marshall(msg);
            forwardingSocket.SendMultipartMessage(mqMsg);
        }
    }
}