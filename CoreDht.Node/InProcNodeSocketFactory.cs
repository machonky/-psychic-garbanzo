using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class InProcNodeSocketFactory : INodeSocketFactory 
    {
        public DealerSocket CreateBindingSocket(string hostAndPort)
        {
            return new DealerSocket(BindingConnectionString(hostAndPort));
        }

        public string BindingConnectionString(string hostAndPort)
        {
            return $"@inproc://{hostAndPort}";
        }

        public DealerSocket CreateForwardingSocket(string hostAndPort)
        {
            return new DealerSocket(ForwardingConnectionString(hostAndPort));
        }

        public string ForwardingConnectionString(string hostAndPort)
        {
            return $">inproc://{hostAndPort}";
        }
    }
}