using System.Net;
using NetworkRouting;

namespace RoutingUtils
{
    public class DnsProvider : IDnsProvider
    {
        public IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            return Dns.GetHostEntry(hostNameOrAddress);
        }
    }
}