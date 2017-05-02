using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    public class NodeMessage : Message, INodeMessage
    {
        public NodeMessage(NodeInfo @from, NodeInfo to)
        {
            From = @from;
            To = to;
        }

        public NodeInfo From { get; }
        public NodeInfo To { get; }
    }
}