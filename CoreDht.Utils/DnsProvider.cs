using System.Net;

namespace CoreDht.Utils
{
    public class DnsProvider : IDnsProvider
    {
        public IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            return Dns.GetHostEntry(hostNameOrAddress);
        }
    }
}