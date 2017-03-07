using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;
using NetworkRouting;
using Routing;

namespace CoreDht
{
    public partial class Node : IPublisher<RoutableMessage>, IDisposable, IOutgoingSocket
    {
        protected MemoryBus MessageBus { get; }
        protected DisposableStack Janitor { get; }
        protected NetMQActor Actor { get; }

        private IMessageSerializer Serializer { get; }

        private PairSocket Shim;
        private NetMQPoller Poller { get; }
        private RoutableMessageMarshaller Marshaller { get; }
        private FingerTable FingerTable { get; }
        private INodeSocketFactory SocketFactory { get; }
        private IConsistentHashingService HashingService { get; }
        private SocketCache ForwardingSockets { get; }
        private DealerSocket ListeningSocket { get; }
        public NetMQTimer InitTimer { get; set; }


        protected Node(NodeInfo identity, IMessageSerializer serializer, INodeSocketFactory socketFactory, IConsistentHashingService hashingService)
        {
            Serializer = serializer;
            Marshaller = new RoutableMessageMarshaller(Serializer);
            MessageBus = new MemoryBus(null, x => new temp.HeirarchicalPublishingStrategy(x));
            MessageBus.Subscribe(new NodeHandler(this));
            Identity = identity;
            Successor = identity;
            SocketFactory = socketFactory;
            HashingService = hashingService;
            Janitor = new DisposableStack();

            Poller = Janitor.Push(new NetMQPoller());
            InitTimer = CreateInitTimer();
            ListeningSocket = Janitor.Push(SocketFactory.CreateBindingSocket(Identity.HostAndPort));
            InitListeningSocket();
            ForwardingSockets = Janitor.Push(new SocketCache(SocketFactory));
            Actor = Janitor.Push(CreateActor());
            InitTimer.Elapsed += (sender, args) =>
            {
                Publish(new NodeReady(Identity.RoutingHash));
            };
            FingerTable = new FingerTable(Identity, Actor);
        }

        private NetMQTimer CreateInitTimer()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var randomStart = TimeSpan.FromMilliseconds(rnd.Next(500, 1000));
            var result = new NetMQTimer(randomStart);
            result.Elapsed += (sender, args) =>
            {
                args.Timer.Enable = false; // one shot only
            };

            return result;
        }

        private void InitListeningSocket()
        {
            ListeningSocket.ReceiveReady += (sender, args) =>
            {
                Actor.SendMultipartMessage(args.Socket.ReceiveMultipartMessage());
            };
        }

        public static string CreateIdentifier(string hostNameOrAddress, int port, int vNodeIndex = -1)
        {
            var vNodeId = vNodeIndex >= 0 ? $"/{vNodeIndex}" : string.Empty;
            var hostAndPort = CreateHostAndPort(hostNameOrAddress, port);
            return $"chord://{hostAndPort}{vNodeId}";
        }

        public static string CreateIdentifier(string hostAndPort, int vNodeIndex = -1)
        {
            var vNodeId = vNodeIndex >= 0 ? $"/{vNodeIndex}" : string.Empty;
            return $"chord://{hostAndPort}{vNodeId}";
        }

        public static string CreateHostAndPort(string hostNameOrAddress, int port)
        {
            return $"{hostNameOrAddress}:{port}";
        }

        private NetMQActor CreateActor()
        {
            return NetMQActor.Create(shim =>
            {
                Shim = shim;
                Shim.ReceiveReady += ShimOnReceiveReady;

                Poller.Add(ListeningSocket);
                Poller.Add(InitTimer);
                Poller.Add(Shim);
                Shim.SignalOK();
                Poller.Run();
            });
        }

        private void ShimOnReceiveReady(object sender, NetMQSocketEventArgs args)
        {
            var mqMsg = args.Socket.ReceiveMultipartMessage();
            if (mqMsg[0].ConvertToString() != NetMQActor.EndShimMessage)
            {
                ConsistentHash hash;
                RoutableMessage msg;
                Marshaller.Unmarshall(mqMsg, out hash, out msg);
                if (msg != null && IsInDomain(hash))
                {
                    MessageBus.Publish(msg);
                }
            }
        }

        private bool IsInDomain(ConsistentHash hash)
        {
            return IsIDInDomain(hash, Identity.RoutingHash, Successor.RoutingHash);
        }

        public static bool IsIDInDomain(ConsistentHash id, ConsistentHash start, ConsistentHash end)
        {
            if (start < end)
            {
                if (id > start && id <= end)
                {
                    return true;
                }
            }
            else //wraparound
            {
                if (id > start || id <= end)
                {
                    return true;
                }
            }

            return false;
        }

        public NodeInfo Identity { get; }

        private NodeInfo Successor { get; set; }

        public void Publish(RoutableMessage message)
        {
            Marshaller.Send(message, Actor);
        }

        public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
        {
            return Actor.TrySend(ref msg, timeout, more);
        }

        private NodeInfo FindClosestPrecedingFinger(ConsistentHash toNode)
        {
            return FingerTable.FindClosestPrecedingFinger(toNode);
        }

        // To be made private

        // temporary
        public JoinNetwork EmitJoinNetwork()
        {
            return new JoinNetwork(Identity, Identity.RoutingHash, Guid.NewGuid());
        }

        #region IDisposable Support

        private bool isDisposed = false;

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (!isDisposed)
            {
                Janitor.Dispose();
                isDisposed = true;
            }
        }

        #endregion

        //temporary
        private void Go()
        {
            var msg = EmitJoinNetwork();
            MessageBus.Publish(new AwaitingJoin(msg.CorrelationId));

            var forwardingSocket = ForwardingSockets[CreateHostAndPort("Touchy", 9000)];
            Marshaller.Send(msg, forwardingSocket);
        }

        public override string ToString()
        {
            return Identity.ToString();
        }
    }

    namespace temp
    {
        // To be fixed in CoreMemoryBus
        public class HeirarchicalPublishingStrategy : PublishingStrategy, IPublishingStrategy
        {
            public HeirarchicalPublishingStrategy(MessageHandlerDictionary messageHandlers)
              : base(messageHandlers)
            {
            }

            public void Publish(Message message)
            {
                Type msgType = message.GetType();
                this.PublishToProxies(message, msgType);
                do
                {
                    msgType = msgType.BaseType;
                    this.PublishToProxies(message, msgType);
                }
                while (msgType != typeof(Message));
            }
        }

        public class PublishingStrategy
        {
            protected MessageHandlerDictionary MessageHandlers { get; private set; }

            protected PublishingStrategy(MessageHandlerDictionary messageHandlers)
            {
                this.MessageHandlers = messageHandlers;
            }

            protected void PublishToProxies(Message message, Type msgType)
            {
                MessageHandlerProxies messageHandlerProxies;
                if (!this.MessageHandlers.TryGetValue(msgType, out messageHandlerProxies))
                {
                    return;
                }

                var proxyCopy = new List<IMessageHandlerProxy>(messageHandlerProxies);
                proxyCopy.ForEach(x => x.Publish(message));
            }
        }
    }
}