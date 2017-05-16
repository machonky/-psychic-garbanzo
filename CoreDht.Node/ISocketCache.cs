namespace CoreDht.Node
{
    public interface ISocketCache
    {
        OutgoingSocket this[string hostAndPort] { get; }
    }
}