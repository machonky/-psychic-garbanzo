using System.Threading;
using NetMQ.Sockets;

namespace Routing
{
    public class InProcNodeSocketFactory : INodeSocketFactory 
    {
        public DealerSocket CreateBindingSocket(string hostAndPort)
        {
            return new DealerSocket(BindingConnectionString(hostAndPort));
        }

//        private volatile int _socketIndex = -1;

        public string BindingConnectionString(string hostAndPort)
        {
            //            Interlocked.Increment(ref _socketIndex);
            //return $"@inproc://{hostAndPort}/{_socketIndex}";
            return $"@inproc://{hostAndPort}";
        }

        public DealerSocket CreateForwardingSocket(string hostAndPort)
        {
            var connectionString = ForwardingConnectionString(hostAndPort);
            return new DealerSocket(ForwardingConnectionString(hostAndPort));
        }

        public string ForwardingConnectionString(string hostAndPort)
        {
            //Interlocked.Increment(ref _socketIndex);
            //return $">inproc://{hostAndPort}/{_socketIndex}";
            return $">inproc://{hostAndPort}";
        }
    }
}