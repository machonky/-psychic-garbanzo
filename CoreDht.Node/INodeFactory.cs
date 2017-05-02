namespace CoreDht.Node
{
    public interface INodeFactory
    {
        Node CreateNode(string uniqueIdentifier, string hostAndPort);
    }
}