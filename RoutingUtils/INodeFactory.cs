namespace CoreDht
{
    public interface INodeFactory
    {
        Node CreateNode(string uniqueIdentifier);
    }
}