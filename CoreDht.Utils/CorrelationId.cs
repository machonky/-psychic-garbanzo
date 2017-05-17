// http://www.singular.co.nz
// Based on Dave Transom's CSharpVitamins - now modified to use Bitcoin base58

using System;

namespace CoreDht.Utils
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a 
    /// shorter string value.
    /// </summary>
    public struct CorrelationId
    {
        #region Static

        /// <summary>
        /// A read-only instance of the CorrelationId class whose value 
        /// is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly CorrelationId Empty = new CorrelationId(Guid.Empty);

        #endregion

        #region Fields
        Guid _guid;
        string _value;

        #endregion

        #region Contructors

        /// <summary>
        /// Creates a CorrelationId from a base58 encoded string
        /// </summary>
        /// <param name="value">The encoded guid as a 
        /// base58 string</param>
        /// 
        public CorrelationId(string value)
        {
            _value = value;
            _guid = Decode(value);
        }

        /// <summary>
        /// Creates a CorrelationId from a Guid
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public CorrelationId(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the underlying Guid
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value != _guid)
                {
                    _guid = value;
                    _value = Encode(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the underlying base58 encoded string
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    _guid = Decode(value);
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the base58 encoded guid as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Returns a value indicating whether this instance and a 
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is CorrelationId)
                return _guid.Equals(((CorrelationId)obj)._guid);
            if (obj is Guid)
                return _guid.Equals((Guid)obj);
            if (obj is string)
                return _guid.Equals(((CorrelationId)obj)._guid);
            return false;
        }

        #endregion

        #region GetHashCode

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion

        #region NewId

        /// <summary>
        /// Initialises a new instance of the CorrelationId class
        /// </summary>
        /// <returns></returns>
        public static CorrelationId NewId()
        {
            return new CorrelationId(Guid.NewGuid());
        }

        #endregion

        #region Encode

        /// <summary>
        /// Creates a new instance of a Guid using the string value, 
        /// then returns the base58 encoded version of the Guid.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a CorrelationId)</param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            Guid guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base58 string
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        /// <returns></returns>
        public static string Encode(Guid guid)
        {
            return Base58Check.Base58CheckEncoding.Encode(guid.ToByteArray());
        }

        #endregion

        #region Decode

        /// <summary>
        /// Decodes the given base58 string
        /// </summary>
        /// <param name="value">The base58 encoded string of a Guid</param>
        /// <returns>A new Guid</returns>
        public static Guid Decode(string value)
        {
            byte[] buffer = Base58Check.Base58CheckEncoding.Decode(value);
            return new Guid(buffer);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both CorrelationIds have the same underlying 
        /// Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(CorrelationId x, CorrelationId y)
        {
            if ((object)x == null) return (object)y == null;
            return x._guid == y._guid;
        }

        /// <summary>
        /// Determines if both CorrelationIds do not have the 
        /// same underlying Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(CorrelationId x, CorrelationId y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the CorrelationId to it's string equivilent
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public static implicit operator string(CorrelationId correlationId)
        {
            return correlationId._value;
        }

        /// <summary>
        /// Implicitly converts the CorrelationId to it's Guid equivilent
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public static implicit operator Guid(CorrelationId correlationId)
        {
            return correlationId._guid;
        }

        /// <summary>
        /// Implicitly converts the string to a CorrelationId
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public static implicit operator CorrelationId(string correlationId)
        {
            return new CorrelationId(correlationId);
        }

        /// <summary>
        /// Implicitly converts the Guid to a CorrelationId 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static implicit operator CorrelationId(Guid guid)
        {
            return new CorrelationId(guid);
        }

        #endregion
    }
}
