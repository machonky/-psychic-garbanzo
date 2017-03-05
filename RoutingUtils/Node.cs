using System;
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

        protected Node(NodeInfo identity, IMessageSerializer serializer, INodeSocketFactory socketFactory, IConsistentHashingService hashingService)
        {
            Serializer = serializer;
            Marshaller = new RoutableMessageMarshaller(Serializer);
            MessageBus = new MemoryBus();
            InitNodeHandlers();
            Identity = identity;
            Successor = identity;
            SocketFactory = socketFactory;
            HashingService = hashingService;
            Janitor = new DisposableStack();
            ForwardingSockets = Janitor.Push(new SocketCache(SocketFactory));
            Poller = Janitor.Push(new NetMQPoller());
            ListeningSocket = Janitor.Push(SocketFactory.CreateBindingSocket(Identity.HostAndPort));
            InitListeningSocket();
            Actor = Janitor.Push(CreateActor());
            FingerTable = new FingerTable(Identity, Actor);
        }

        private void InitNodeHandlers()
        {
            MessageBus.Subscribe(new NodeHandler(this));
        }

        private void InitListeningSocket()
        {
            ListeningSocket.ReceiveReady += (sender, args) =>
            {
                Actor.SendMultipartMessage(args.Socket.ReceiveMultipartMessage());
            };
            Poller.Add(ListeningSocket);
        }

        public static string CreateIdentifier(string hostNameOrAddress, int port, int vNodeIndex = -1)
        {
            var vNodeId = vNodeIndex >= 0 ? $"/{vNodeIndex}" : string.Empty;
            var hostAndPort = CreateHostAndPort(hostNameOrAddress, port);
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
                Shim = Janitor.Push(shim);
                Shim.ReceiveReady += (sender, args) =>
                {
                    var mqMsg = args.Socket.ReceiveMultipartMessage(3);
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
                };

                Poller.Add(Shim);
                Shim.SignalOK();
                Poller.Run();
            });
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
        public JoinNetwork EmitJoinNetwork(string hostNameOrAddress, int port, int vNodeIndex = -1)
        {
            string identifier = CreateIdentifier(hostNameOrAddress, port, vNodeIndex);
            string hostAndPort = CreateHostAndPort(hostNameOrAddress, port);
            var routingHash = HashingService.GetConsistentHash(identifier); 
            var msg = new JoinNetwork(new NodeInfo(identifier, routingHash, hostAndPort), HashingService.GetConsistentHash(identifier),Guid.NewGuid());
            return msg;
        }


        #region IDisposable Support

        private bool isDisposed = false;

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (!isDisposed)
            {
                Janitor.Dispose();
            }
        }

        #endregion
    }
}