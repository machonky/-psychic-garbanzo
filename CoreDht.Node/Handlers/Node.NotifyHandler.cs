using CoreDht.Node.Messages.NetworkMaintenance;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class NotifyHandler
            : IHandle<Notify>
        {
            private readonly Node _node;
            private readonly ICommunicationManager _commMgr;

            public NotifyHandler(Node node, ICommunicationManager commMgr)
            {
                _node = node;
                _commMgr = commMgr;
            }

            public void Handle(Notify message)
            {
                _node.LogMessage(message);
                _commMgr.SendAck(message, message.CorrelationId);

                // Let the predecessor know to join to the specified successor.
                // And adjust successor table accordingly.
                var entries = _node.SuccessorTable.Entries;
                for (int i = 1; i < entries.Length; ++i)
                {
                    entries[i] = entries[i - 1];
                }
                var successor = message.NewSuccessor;                
                entries[0] = new RoutingTableEntry(successor.RoutingHash, successor);

                _node.Log($"Changed successor to {successor}");
                _commMgr.Send(new NotifyReply(_node.Identity, message.From, message.CorrelationId));
            }
        }
    }
}
