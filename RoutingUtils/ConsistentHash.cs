using System;
using System.Numerics;
using Routing;

namespace CoreDht
{
    /// <summary>
    /// We really need to deprecate the BigInteger implementation here, in favour of something fast...
    /// </summary>
    public class ConsistentHash : IComparable<ConsistentHash>, ICloneable<ConsistentHash>
    {
        private BigInteger _bigKey;
        private BigInteger _bigKeyMax;
        private const int BitsPerByte = 8;

        public ConsistentHash()
        {
            Init(new byte[1]);
        }

        public ConsistentHash(byte[] hashBytes)
        {
            Init(hashBytes);
        }

        public ConsistentHash(BigInteger bigInteger) : this(bigInteger.ToByteArray())
        { }

        private void Init(byte[] hashBytes)
        {
            BitCount = hashBytes.Length*BitsPerByte;
            _bigKeyMax = new BigInteger(1) << BitCount;

            hashBytes = EnsureUnsigned(hashBytes);
            _bigKey = new BigInteger(hashBytes);
        }

        private static byte[] EnsureUnsigned(byte[] hashBytes)
        {
            if ((hashBytes[hashBytes.Length - 1] & 0x80) > 0)
            {
                byte[] temp = new byte[hashBytes.Length];
                Array.Copy(hashBytes, temp, hashBytes.Length);
                hashBytes = new byte[hashBytes.Length + 1];
                Array.Copy(temp, hashBytes, temp.Length);
            }
            return hashBytes;
        }

        public int BitCount { get; private set; }

        public byte[] Bytes
        {
            get { return _bigKey.ToByteArray(); }
            set { Init(value); }
        }

        public override int GetHashCode()
        {
            return _bigKey.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConsistentHash;
            if (other != null)
            {
                return _bigKey.Equals(other._bigKey);
            }

            return false;
        }

        public int CompareTo(ConsistentHash other)
        {
            return other._bigKey.CompareTo(_bigKey);
        }

        public ConsistentHash Clone()
        {
            return new ConsistentHash(Bytes);
        }

        public override string ToString()
        {
            return _bigKey.ToString();
        }

        public static bool operator <(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs._bigKey < rhs._bigKey;
        }

        public static bool operator >(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs._bigKey > rhs._bigKey;
        }
        public static bool operator <=(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs._bigKey <= rhs._bigKey;
        }

        public static bool operator >=(ConsistentHash lhs, ConsistentHash rhs)
        {
            return lhs._bigKey >= rhs._bigKey;
        }

        public static ConsistentHash operator +(ConsistentHash lhs, ConsistentHash rhs)
        {
            return new ConsistentHash((lhs._bigKey + rhs._bigKey) % lhs._bigKeyMax);
        }

        public static readonly ConsistentHash Zero = new ConsistentHash(0);

        public static ConsistentHash BitValue(int i)
        {
            return new ConsistentHash(new BigInteger(1) << i);
        }
    }
}