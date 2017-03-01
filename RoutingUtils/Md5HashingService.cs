

using System.Security.Cryptography;

namespace Routing
{
    public class Md5HashingService : ConsistentHashingService
    {
        public Md5HashingService() : base(new MD5CryptoServiceProvider())
        { }
    }
}