using System;
using System.Numerics;
using NUnit.Framework;
using CoreDht;
using Routing;

namespace Tests
{
 [TestFixture]
    public class Class1
    {
        [Test]
        public void TestToStringFromString()
        {
            var x = new ConsistentHash(new byte[2] {0xff,0xfe});
            Console.WriteLine(x.ToString());
//            Assert.That(x.ToString(), Is.EqualTo("fffe"));
            var y = ConsistentHash.New("fffe");
//            Assert.True(x == y);
        }

        [Test]
        public void TestConsistentHash()
        {
            var gizmo = new Md5HashingService();
            var hash = gizmo.GetConsistentHash("Hello world");
            Console.WriteLine(hash);
            var bigInt = new BigInteger(hash.Bytes);
            Console.WriteLine(bigInt.ToString("X"));
        }

        [Test]
        public void TestCompare()
        {
            var x = new ConsistentHash(new byte[2] { 0xff, 0xfe });
            var y = new ConsistentHash(new byte[2] { 0xff, 0x00 });
            Assert.True(x > y);
        }

    }
}
