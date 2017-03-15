using NetMQ;
using NetworkRouting;
using Routing;

namespace CoreDht
{
    public class NodeMarshaller
    {
        private readonly IMessageSerializer _serializer;
        private readonly IConsistentHashingService _hashingService;

        public const string NodeMessage = "NM";
        public const string NodeReply = "NR";
        public const string RoutableMessage = "RM";

        public NodeMarshaller(IMessageSerializer serializer, IConsistentHashingService hashingService)
        {
            _serializer = serializer;
            _hashingService = hashingService;
        }

        private static readonly byte[] EmptyByteArray = new byte[0];

        public NetMQMessage Marshall(RoutableMessage msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(RoutableMessage),
                new NetMQFrame(EmptyByteArray),
                new NetMQFrame(msg.RoutingTarget.Bytes),
                new NetMQFrame(EmptyByteArray),
                new NetMQFrame(json),
            });

            return result;
        }

        public NetMQMessage Marshall(NodeMessage msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(NodeMessage),
                new NetMQFrame(EmptyByteArray),
                new NetMQFrame(json),
            });

            return result;
        }

        public NetMQMessage Marshall(NodeReply msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(NodeReply),
                new NetMQFrame(EmptyByteArray),
                new NetMQFrame(json),
            });

            return result;
        }

        public void Send(RoutableMessage msg, IOutgoingSocket outgoingSocket)
        {
            var mqMsg = Marshall(msg);
            outgoingSocket.SendMultipartMessage(mqMsg);
        }

        public void Send(NodeMessage msg, IOutgoingSocket outgoingSocket)
        {
            var mqMsg = Marshall(msg);
            outgoingSocket.SendMultipartMessage(mqMsg);
        }

        public void Send(NodeReply msg, IOutgoingSocket outgoingSocket)
        {
            var mqMsg = Marshall(msg);
            outgoingSocket.SendMultipartMessage(mqMsg);
        }

        public void Unmarshall(NetMQMessage netMqMessage, out ConsistentHash routingHash, out RoutableMessage msg)
        {
            routingHash = new ConsistentHash(bytes: netMqMessage[2].ToByteArray());

            msg = (RoutableMessage)_serializer.Deserialize(json: netMqMessage[4].ConvertToString());
        }

        public void Unmarshall(NetMQMessage netMqMessage, out NodeMessage msg)
        {
            msg = (NodeMessage)_serializer.Deserialize(json: netMqMessage[2].ConvertToString());
        }

        public void Unmarshall(NetMQMessage netMqMessage, out NodeReply msg)
        {
            msg = (NodeReply)_serializer.Deserialize(json: netMqMessage[2].ConvertToString());
        }
    }
}