using System.Security.Cryptography;

namespace CoreDht
{
    public class Sha256HashingService : ConsistentHashingService
    {
        public Sha256HashingService() : base(SHA256.Create())
        { }
    }
}