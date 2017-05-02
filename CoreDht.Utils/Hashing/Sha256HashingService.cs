using System.Security.Cryptography;

namespace CoreDht.Utils.Hashing
{
    public class Sha256HashingService : ConsistentHashingService
    {
        public Sha256HashingService() : base(SHA256.Create())
        { }
    }
}