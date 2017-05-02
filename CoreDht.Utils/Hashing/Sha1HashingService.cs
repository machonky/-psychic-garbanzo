using System.Security.Cryptography;

namespace CoreDht.Utils.Hashing
{
    public class Sha1HashingService : ConsistentHashingService
    {
        public Sha1HashingService() : base(SHA1.Create())
        { }
    }
}