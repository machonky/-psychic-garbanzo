using System;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;
using NetworkRouting;

namespace CoreDht
{
    public class Node : IPublisher<RoutableMessage>, IDisposable, IOutgoingSocket
    {
        protected MemoryBus MessageBus { get; }
        protected DisposableStack Janitor { get; }
        protected NetMQActor Actor { get; }

        private IMessageSerializer Serializer { get; }

        private PairSocket Shim;
        private NetMQPoller Poller;
        private RoutableMessageMarshaller Marshaller;
        private FingerTable FingerTable;

        protected Node(NodeInfo identity, IMessageSerializer serializer)
        {
            Serializer = serializer;
            Marshaller = new RoutableMessageMarshaller(Serializer);
            MessageBus = new MemoryBus();
            MessageBus.Subscribe(new NodeHandler(this));
            Identity = identity;
            FingerTable = new FingerTable(Identity);
            Janitor = new DisposableStack();
            Actor = Janitor.Push(CreateActor());
        }

        public static string CreateIdentifier(string hostNameOrAddress, int port)
        {
            return $"chord://{hostNameOrAddress}:{port}";
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

                Poller = Janitor.Push(new NetMQPoller { Shim });
                Shim.SignalOK();
                Poller.Run();
            });
        }

        private bool IsInDomain(ConsistentHash hash)
        {
            return true;
        }

        class NodeHandler : 
            IHandle<JoinNetwork>, 
            IHandle<JoinNetworkReply>,
            IHandle<TerminateNode>
        {
            private readonly Node Node;

            public NodeHandler(Node node)
            {
                Node = node;
            }

            public void Handle(JoinNetwork message)
            {
                // Another node wishes to join our network.
                // We need to find it's successor so it can make a connection, and insert itself into the network ring
            }

            public void Handle(JoinNetworkReply message)
            {
                // Connect to the network based on the reply information
            }

            public void Handle(TerminateNode message)
            {
                if (Node.Identity.RoutingHash.Equals(message.RoutingTarget))
                {
                    Node.Poller.Stop();
                }
            }
        }

        public NodeInfo Identity { get; }

        public void Publish(RoutableMessage message)
        {
            var mqMsg = Marshaller.Marshall(message);
            Actor.SendMultipartMessage(mqMsg);
        }

        public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
        {
            return Actor.TrySend(ref msg, timeout, more);
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