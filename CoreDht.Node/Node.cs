using System;
using System.Collections.Generic;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public partial class Node : IDisposable
    {
        public NodeInfo Identity { get; }
        public NodeConfiguration Config { get; }
        public NodeInfo Predecessor { get; private set; }
        protected IUtcClock Clock { get; }
        protected ICorrelationFactory<CorrelationId> CorrelationFactory { get; }
        protected MemoryBus MessageBus { get; }
        protected DisposableStack Janitor { get; }
        private NetMQPoller Poller { get; }
        protected NetMQActor Actor { get; }
        private PairSocket Shim { get; set; }
        private Action<string> Logger { get; }
        private SocketCache ForwardingSockets { get; }
        private DealerSocket ListeningSocket { get; }
        private FingerTable FingerTable { get; }
        private SuccessorTable SuccessorTable { get; }
        private IActionScheduler ActionScheduler { get; }
        private IExpiryTimeCalculator ExpiryCalculator { get; }
        private INodeMarshaller Marshaller { get; }

        public NodeInfo Successor => SuccessorTable[0].SuccessorIdentity;

        protected Node(NodeInfo identity, NodeConfiguration config)
        {
            Config = config;
            Identity = identity;
            Predecessor = Identity;

            Clock = Config.Clock;
            CorrelationFactory = Config.CorrelationFactory;
            Janitor = new DisposableStack();
            MessageBus = new MemoryBus();
            Poller = Janitor.Push(new NetMQPoller());
            Logger = Config.LoggerDelegate;
            Marshaller = Config.MarshallerFactory.Create();

            ExpiryCalculator = Config.ExpiryCalculator;

            Log($"Binding to {Config.NodeSocketFactory.BindingConnectionString(Identity.HostAndPort)}");
            ListeningSocket = Janitor.Push(Config.NodeSocketFactory.CreateBindingSocket(Identity.HostAndPort));

            ForwardingSockets = Janitor.Push(new SocketCache(Config.NodeSocketFactory, Clock)); // Chicken and egg scenario where we require an actor!

            Actor = Janitor.Push(CreateActor());
            ForwardingSockets.AddActor(Identity.HostAndPort, Actor);
            Janitor.Push(new DisposableAction(() => { Poller.Remove(ListeningSocket); }));

            Janitor.Push(new DisposableAction(
                () => { ListeningSocket.ReceiveReady += ListeningSocketReceiveReady; },
                () => { ListeningSocket.ReceiveReady -= ListeningSocketReceiveReady; }));

            ActionScheduler = Janitor.Push(new NodeActionScheduler(Clock, Config.ActionTimerFactory.Create(), Marshaller, Actor));
            MessageBus.Subscribe(new NodeHandler(this, ActionScheduler));
            var awaitHandler = Janitor.Push(new AwaitAckHandler(ActionScheduler, ExpiryCalculator, Marshaller, Actor, Log, Config.AwaitSettings));
            MessageBus.Subscribe(awaitHandler);
            MessageBus.Subscribe(new JoinNetworkHandler(this));
            MessageBus.Subscribe(new StabilizeHandler(this));
            MessageBus.Subscribe(new RectifyHandler(this));

            FingerTable = new FingerTable(Identity, Identity.RoutingHash.BitCount);
            SuccessorTable = new SuccessorTable(Identity, Config.SuccessorCount);

            // Let everything know we're ready to go.
            Log($"Sending NodeInitialised");
            Marshaller.Send(new NodeInitialised(), Actor);
        }

        protected virtual void OnInitialised()
        {
            var seedNode = Config.SeedNode;
            if (!string.Equals(Identity.HostAndPort, seedNode))
            {
                var min = Config.JoinSettings.JoinMinTimeout;
                var max = Config.JoinSettings.JoinMaxTimeout;
                var timeout = Config.Random.Next(min, max);

                Log($"Join Timeout {timeout} ms");

                var dueTime = ExpiryCalculator.CalcExpiry(timeout);
                ActionScheduler.ScheduleAction(dueTime, null, _ => { InitJoin(); });
            }
        }

        private void InitJoin()
        {
            var socket = ForwardingSockets[Identity.HostAndPort];
            Marshaller.Send(new InitJoin(), socket);
        }

        private void InitStabilize()
        {
            var socket = ForwardingSockets[Identity.HostAndPort];
            Marshaller.Send(new InitStabilize(), socket);
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
        
        internal void Log(string logMsg)
        {
            Logger?.Invoke($"{Identity} {logMsg}");
        }

        internal AwaitAllResponsesHandler CreateAwaitAllResponsesHandler()
        {
            return new AwaitAllResponsesHandler(MessageBus,MessageBus,Log);
        }

        private NodeInfo CalcNodeInfo(string hostAndPort)
        {
            var identifier = CreateIdentifier(hostAndPort);
            var routingHash = Config.HashingService.GetConsistentHash(identifier);
            return new NodeInfo(identifier, routingHash, hostAndPort);
        }

        private void ListeningSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                OnReceiveListeningSocket(e);
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex}");
            }    
        }

        protected virtual void OnReceiveListeningSocket(NetMQSocketEventArgs args)
        {
            Actor.SendMultipartMessage(args.Socket.ReceiveMultipartMessage());
        }

        private NetMQActor CreateActor()
        {
            return NetMQActor.Create(shim =>
            {
                Shim = shim;
                Shim.ReceiveReady += ShimOnReceiveReady;
                Shim.SignalOK();

                Poller.Add(Shim);
                Poller.Add(ListeningSocket);
                Poller.Run();

                Log("Node closed");
            });
        }

        private void ShimOnReceiveReady(object sender, NetMQSocketEventArgs args)
        {
            // Unmarshall the message to the message bus
            var mqMsg = args.Socket.ReceiveMultipartMessage();
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
                    Log($"Node terminating.");
                    break;
            }
        }

        private void UnMarshallPointToPointMsg(NetMQMessage mqMsg)
        {
            PointToPointMessage msg;
            Marshaller.Unmarshall(mqMsg, out msg);
            MessageBus.Publish(msg);
        }

        private void UnMarshallMessage(NetMQMessage mqMsg)
        {                                                                                                                                                                                                            
            Message msg;
            Marshaller.Unmarshall(mqMsg, out msg);
            MessageBus.Publish(msg);
        }

        private bool IsInDomain(ConsistentHash hash)
        {
            return IsIdInDomain(hash, Identity.RoutingHash, Successor.RoutingHash);
        }

        public static bool IsIdInDomain(ConsistentHash id, ConsistentHash start, ConsistentHash end)
        {
            return id.IsBetween(start, end);
        }

        public override string ToString()
        {
            return Identity.ToString();
        }

        protected void Terminate()
        {
            Poller.Stop();
        }

        protected void SendAckMessage(IPointToPointMessage message, CorrelationId correlationId)
        {
            // Send an interim reply to verify the health of the network
            var replySocket = ForwardingSockets[message.From.HostAndPort];
            Marshaller.Send(new AckMessage(correlationId), replySocket);
        }
        
        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Terminate();
                Janitor.Dispose();
                _isDisposed = true;
            }
        }

        #endregion
    }
}
