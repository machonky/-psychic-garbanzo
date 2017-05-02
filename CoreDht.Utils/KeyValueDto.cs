using CoreDht.Utils;
using CoreDht.Utils.Hashing;

namespace CoreDht
{
    public class KeyValueDto<T>
    {
        public ConsistentHash Key { get; set; }
        public T Value { get; set; }
    }

    public static class KeyValueDto
    {
        public static KeyValueDto<T> New<T>(ConsistentHash key, T value)
        {
            return new KeyValueDto<T> {Key = key, Value = value};
        }
    }
}