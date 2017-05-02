using System.Net;

namespace CoreDht.Utils
{
    public interface IDnsProvider
    {
        IPHostEntry GetHostEntry(string hostNameOrAddress);
    }
}