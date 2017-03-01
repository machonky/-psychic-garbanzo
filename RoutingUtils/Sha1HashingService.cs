using System.Security.Cryptography;

namespace Routing
{
    public class Sha1HashingService : ConsistentHashingService
    {
        public Sha1HashingService() : base(new SHA1CryptoServiceProvider())
        { }
    }
}