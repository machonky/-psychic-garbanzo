using System.Security.Cryptography;

namespace Routing
{
    public class Sha256HashingService : ConsistentHashingService
    {
        public Sha256HashingService() : base(new SHA256CryptoServiceProvider())
        { }
    }
}