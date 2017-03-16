﻿using System;
using System.Threading;
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
        protected string SeedNode { get; }
        protected MemoryBus MessageBus { get; }
        protected DisposableStack Janitor { get; }
        protected NetMQActor Actor { get; }
        private IMessageSerializer Serializer { get; }
        private PairSocket Shim;
        private NetMQPoller Poller { get; }
        private NodeMarshaller Marshaller { get; }
        private FingerTable FingerTable { get; }
        private SuccessorTable SuccessorTable { get; }
        private INodeSocketFactory SocketFactory { get; }
        private IConsistentHashingService HashingService { get; }
        public IClock Clock { get; }
        public ICorrelationFactory<Guid> CorrelationFactory { get; }
        private SocketCache ForwardingSockets { get; }
        private DealerSocket ListeningSocket { get; }
        public NetMQTimer InitTimer { get; set; }

        protected Node(NodeInfo identity, NodeConfiguration config)
        {
            SeedNode = config.SeedNode;
            Serializer = config.Serializer;
            Marshaller = new NodeMarshaller(Serializer, config.HashingService);
            MessageBus = new MemoryBus();
            InitHandlers();
            Identity = identity;
            Successor = identity;
            Predecessor = null;
            SocketFactory = config.NodeSocketFactory;
            HashingService = config.HashingService;
            CorrelationFactory = config.CorrelationFactory;
            Clock = config.Clock;
            Janitor = new DisposableStack();

            Poller = Janitor.Push(new NetMQPoller());
            InitTimer = CreateInitTimer();
            ListeningSocket = Janitor.Push(SocketFactory.CreateBindingSocket(Identity.HostAndPort));
            InitListeningSocket();
            ForwardingSockets = Janitor.Push(new SocketCache(SocketFactory, Clock));
            Actor = Janitor.Push(CreateActor());
            InitTimer.Elapsed += (sender, args) =>
            {
                Publish(new NodeReady(Identity.RoutingHash));
            };
            FingerTable = new FingerTable(Identity, Identity.RoutingHash.BitCount);
            SuccessorTable = new SuccessorTable(Identity, config.SuccessorTableLength);
        }


        private void InitHandlers()
        {
            MessageBus.Subscribe(new NodeHandler(this));
            MessageBus.Subscribe(new AwaitMessageHandler());
            MessageBus.Subscribe(new JoinNetworkHandler(this));
            MessageBus.Subscribe(new GetFingerTableHandler(this));
            MessageBus.Subscribe(new FindSuccessorToHashHandler(this));
        }

        private NetMQTimer CreateInitTimer()
        {
            var guid = CorrelationFactory.GetNextCorrelation();
            var rnd = new Random(guid.GetHashCode());
            var randomStart = TimeSpan.FromMilliseconds(rnd.Next(500, 1000)); // Prevent self similar behaviour
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
                var typeCode = mqMsg[0].ConvertToString();
                switch (typeCode)
                {
                    case NodeMarshaller.RoutableMessage:
                        UnmarshalRoutableMsg(mqMsg);
                        break;
                    case NodeMarshaller.NodeMessage:
                        UnMarshallNode(mqMsg);
                        break;
                    case NodeMarshaller.NodeReply:
                        UnMarshallReply(mqMsg);
                        break;
                }
            }
            Thread.Sleep(0);
        }

        private void UnMarshallReply(NetMQMessage mqMsg)
        {
            NodeReply msg;
            Marshaller.Unmarshall(mqMsg, out msg);
            MessageBus.Publish(msg);
        }
        private void UnMarshallNode(NetMQMessage mqMsg)
        {
            NodeMessage msg;
            Marshaller.Unmarshall(mqMsg, out msg);
            MessageBus.Publish(msg);
        }

        private void UnmarshalRoutableMsg(NetMQMessage mqMsg)
        {
            ConsistentHash hash;
            RoutableMessage msg;
            Marshaller.Unmarshall(mqMsg, out hash, out msg);
            if (msg != null && IsInDomain(hash))
            {
                if (IsInDomain(hash))
                {
                    MessageBus.Publish(msg);
                }
                else
                {
                    // Find a forwarding socket & send it
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

        private NodeInfo Predecessor { get; set; }

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
            return new JoinNetwork(Identity, CorrelationFactory.GetNextCorrelation());
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
            MessageBus.Publish(new JoinNetwork.Await(msg.CorrelationId));
            //var msg = new GetFingerTable(Identity,Identity, CorrelationFactory.GetNextCorrelation());
            //MessageBus.Publish(new GetFingerTable.Await(msg.CorrelationId));

            var forwardingSocket = ForwardingSockets[SeedNode];
            Marshaller.Send(msg, forwardingSocket);
        }

        public override string ToString()
        {
            return Identity.ToString();
        }

        private void SendReply(NodeInfo target, NodeReply reply)
        {
            if (target.Equals(Identity))
            {
                MessageBus.Publish(reply);
            }
            else
            {
                var forwardingSocket = ForwardingSockets[target.HostAndPort];
                Marshaller.Send(reply, forwardingSocket);
            }
        }

        private void CloseHandler(ICorrelatedMessage<Guid> message)
        {
            MessageBus.Publish(new OperationComplete(message.CorrelationId));
        }
    }
}