namespace CoreDht
{
    /// <summary>
    /// Stabilize is an instruction to a node to stabilize/verify its sucessor references
    /// </summary>
    public class Stabilize : NodeMessage
    {
        public Stabilize(NodeInfo identity) : base(identity)
        {}
    }
}