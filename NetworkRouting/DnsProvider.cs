using System.Net;

namespace NetworkRouting
{
    public interface IDnsProvider
    {
        IPHostEntry GetHostEntry(string hostNameOrAddress);
    }

    public class DnsProvider : IDnsProvider
    {
        public IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            return Dns.GetHostEntry(hostNameOrAddress);
        }
    }
}