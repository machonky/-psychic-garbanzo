

using System.Security.Cryptography;

namespace CoreDht
{
    public class Md5HashingService : ConsistentHashingService
    {
        public Md5HashingService() : base(MD5.Create())
        { }
    }
}