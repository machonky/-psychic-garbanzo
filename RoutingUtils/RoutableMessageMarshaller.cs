using CoreMemoryBus.Messaging;
using NetMQ;
using NetworkRouting;

namespace CoreDht
{
    public class RoutableMessageMarshaller
    {
        private readonly IMessageSerializer _serializer;

        public RoutableMessageMarshaller(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        private static readonly byte[] EmptyByteArray = new byte[0];

        public NetMQMessage Marshall(RoutableMessage msg)
        {
            var json = _serializer.Serialize(msg);
            var result = new NetMQMessage(new[]
            {
                new NetMQFrame(msg.RoutingTarget.Bytes),
                new NetMQFrame(EmptyByteArray),
                new NetMQFrame(json),
            });

            return result;
        }

        public void Unmarshall(NetMQMessage netMqMessage, out ConsistentHash routingHash, out RoutableMessage msg)
        {
            routingHash = new ConsistentHash(hashBytes: netMqMessage[0].ToByteArray());
            msg = (RoutableMessage)_serializer.Deserialize(json: netMqMessage[2].ConvertToString());
        }
    }
}