using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class RectifyHandler
            : IHandle<InitRectify>
            , IHandle<Rectify>
            , IHandle<RectifyReply>
        {
            private readonly Node _node;

            public RectifyHandler(Node node)
            {
                _node = node;
            }

            public void Handle(InitRectify message)
            {
                // Notify the successor to update it's predecessor with this node's info
            }

            public void Handle(Rectify message)
            {
                // Assign this node's predecessor to the value supplied.
                // alert the old predecessor to stabilize to the new predecessor
            }

            public void Handle(RectifyReply message)
            {
                // this node is now a live node and can handle application messages.
            }
        }
    }
}
