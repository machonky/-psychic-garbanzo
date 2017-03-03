using System.Net;

namespace NetworkRouting
{
    public interface IDnsProvider
    {
        IPHostEntry GetHostEntry(string hostNameOrAddress);
    }
}