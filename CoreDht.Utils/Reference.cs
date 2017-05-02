namespace CoreDht.Utils
{
    /// <summary>
    /// Reference permits us to contain a value type as a reference. The 
    /// Reference object may then be used anywhere the value type is expected.
    /// This is useful in anonymous functions where the behaviour is controlled 
    /// by a counter defined outside the scope of the closure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Reference<T>
    {
        public T Value { get; set; }

        public Reference(T value)
        {
            Value = value;
        }

        public static implicit operator T(Reference<T> v)
        {
            return v.Value;
        }
    }
}