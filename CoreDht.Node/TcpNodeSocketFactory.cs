using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class TcpNodeSocketFactory : INodeSocketFactory
    {
        public DealerSocket CreateBindingSocket(string hostAndPort)
        {
            return new DealerSocket(BindingConnectionString(hostAndPort));
        }

        public string BindingConnectionString(string hostAndPort)
        {
            return $"@tcp://{hostAndPort}";
        }

        public DealerSocket CreateForwardingSocket(string hostAndPort)
        {
            return new DealerSocket(ForwardingConnectionString(hostAndPort));
        }

        public string ForwardingConnectionString(string hostAndPort)
        {
            return $">tcp://{hostAndPort}";
        }
    }
}