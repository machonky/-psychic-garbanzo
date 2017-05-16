using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    public class PointToPointMessage : Message, IPointToPointMessage
    {
        public PointToPointMessage(NodeInfo @from, NodeInfo to)
        {
            From = @from;
            To = to;
        }

        public NodeInfo From { get; }
        public NodeInfo To { get; set; }
    }
}