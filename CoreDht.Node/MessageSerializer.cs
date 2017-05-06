using CoreMemoryBus.Messages;
using Newtonsoft.Json;

namespace CoreDht.Node
{
    public class MessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public string Serialize(Message message)
        {
            return JsonConvert.SerializeObject(message, _settings);
        }

        public TMessage Deserialize<TMessage>(string json) where TMessage:Message
        {
            return JsonConvert.DeserializeObject<TMessage>(json, _settings);
        }
    }
}