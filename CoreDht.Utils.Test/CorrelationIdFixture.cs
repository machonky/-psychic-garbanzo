using System;
using CoreDht.Node.Messages.NetworkMaintenance;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CoreDht.Utils.Test
{
    [TestFixture]
    public class CorrelationIdFixture
    {
        [Test]
        public void TestSerialization()
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var id0 = new CorrelationId(Guid.Empty);
            var json = JsonConvert.SerializeObject(id0, settings);
            var id1 = JsonConvert.DeserializeObject<CorrelationId>(json, settings);

            Assert.That(id0, Is.EqualTo(id1));
        }
    }
}
