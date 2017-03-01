using CoreMemoryBus.Messages;
using Newtonsoft.Json;

namespace NetworkRouting
{
    public class MessageSerializer : IMessageSerializer
    {
        JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public string Serialize(Message message)
        {
            return JsonConvert.SerializeObject(message, _settings);
        }

        public Message Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json, _settings);
        }
    }
}