using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// NodeMessage and derived messages are point-to-point messages between 
    /// nodes on a structured overlay network to manage node connectivity.
    /// Where the point-to-point connection can no longer be created, the 
    /// message can still be routed via the routing hash in the NodeInfo with 
    /// NodeMessageExtensions.ToRoutableMsg extension method. 
    /// </summary>
    public class NodeMessage : Message
    {
        protected NodeMessage(NodeInfo identity)
        {
            Identity = identity;
        }

        public NodeInfo Identity { get; }
    }

    public class NodeReply : Message
    {
        public NodeInfo Identity { get; }

        protected NodeReply(NodeInfo identity)
        {
            Identity = identity;
        }
    }

    //public static class NodeMessageExtensions
    //{
    //    public static RoutableNodeMessage ToRoutableMsg(this NodeMessage msg)
    //    {
    //        return new RoutableNodeMessage(msg);
    //    }
    //}

    ///// <summary>
    ///// A RoutableNodeMessage is transmitted when a point-to-point message cannot be sent 
    ///// </summary>
    //public class RoutableNodeMessage : RoutableMessage
    //{
    //    public RoutableNodeMessage(NodeMessage msg) : base(msg.Identity.RoutingHash)
    //    {
    //        Message = msg;

    //    }

    //    public NodeMessage Message { get; }
    //}
}