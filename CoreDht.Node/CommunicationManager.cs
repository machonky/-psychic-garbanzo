using System;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;

namespace CoreDht.Node
{
    public class CommunicationManager : ICommunicationManager
    {
        private readonly NodeInfo _node;
        private readonly INodeMarshaller _marshaller;
        private readonly ISocketCache _socketCache;
        private readonly IPublisher _publisher;
        private readonly Action<string> _logger;

        public CommunicationManager(NodeInfo node, INodeMarshaller marshaller, ISocketCache socketCache, IPublisher publisher, Action<string> logger)
        {
            _node = node;
            _marshaller = marshaller;
            _socketCache = socketCache;
            _publisher = publisher;
            _logger = logger;
        }

        public void SendInternal(Message msg)
        {
            var replySocket = _socketCache[_node.HostAndPort];
            _marshaller.Send(msg, replySocket);
        }

        public void Send(PointToPointMessage msg)
        {
            var replySocket = _socketCache[msg.To.HostAndPort];
            _marshaller.Send(msg, replySocket);
        }

        public void SendAck(PointToPointMessage originatingMessage, CorrelationId correlation)
        {
            // Send an interim reply to verify the health of the network
            var replySocket = _socketCache[originatingMessage.From.HostAndPort];
            _marshaller.Send(new AckMessage(correlation), replySocket);
        }

        public void Receive(NetMQMessage mqMsg)
        {
            var typeCode = mqMsg[0].ConvertToString();
            switch (typeCode)
            {
                //case NodeMarshaller.RoutableMessage:
                //    UnmarshalRoutableMsg(mqMsg);
                //    break;
                case NodeMarshaller.PointToPointMessage:
                    UnMarshallPointToPointMsg(mqMsg);
                    break;
                case NodeMarshaller.InternalMessage:
                    UnMarshallMessage(mqMsg);
                    break;
                case NetMQActor.EndShimMessage:
                    _logger?.Invoke($"Node terminating.");
                    break;
            }
        }

        private void UnMarshallMessage(NetMQMessage mqMsg)
        {
            Message msg;
            _marshaller.Unmarshall(mqMsg, out msg);
            _publisher.Publish(msg);
        }

        private void UnMarshallPointToPointMsg(NetMQMessage mqMsg)
        {
            PointToPointMessage msg;
            _marshaller.Unmarshall(mqMsg, out msg);
            _publisher.Publish(msg);
        }
    }
}