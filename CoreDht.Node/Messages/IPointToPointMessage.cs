namespace CoreDht.Node.Messages
{
    /// <summary>
    /// Messages implementing IPointToPointMessage are point to point messages not subject to routing in structured network. 
    /// They are usually involved in communication between nodes in operations that build the structured network.
    /// </summary>
    public interface IPointToPointMessage
    {
        NodeInfo From { get; }
        NodeInfo To { get; set; } // Message may be redirected between hopss
    }
}
