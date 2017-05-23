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
                // Let the predecessor know to join to the specified successor.
                // And adjust successor table accordingly.
            }
        }
    }
}
