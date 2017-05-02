namespace CoreDht.Node.Messages
{
    /// <summary>
    /// Messages implementing INodeMessage are point to point messages. 
    /// They are usually involved in communication between nodes in operations that build the structured network.
    /// </summary>
    public interface INodeMessage
    {
        NodeInfo From { get; }
        NodeInfo To { get; }
    }
}
