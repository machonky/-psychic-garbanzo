using NetMQ.Sockets;

namespace Routing
{
    public interface INodeSocketFactory
    {
        string BindingConnectionString(string hostAndPort);
        DealerSocket CreateBindingSocket(string hostAndPort);
        string ForwardingConnectionString(string hostAndPort);
        DealerSocket CreateForwardingSocket(string hostAndPort);
    }
}