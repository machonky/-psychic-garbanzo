namespace CoreDht
{
    public class NotifyPredecessor : NodeMessage
    {
        public NotifyPredecessor(NodeInfo identity) : base(identity)
        {}
    }
}