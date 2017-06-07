using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        /// <summary>
        /// NodeHandler handles internal correlation free events responsible for node lifetime and basic operation.
        /// </summary>
        public class NodeHandler
            : IHandle<TimerFired>
            , IHandle<NodeInitialised>
            , IHandle<TerminateNode>
        {
            private readonly Node _node;
            private readonly IActionScheduler _actionScheduler;

            public NodeHandler(Node node, IActionScheduler actionScheduler)
            {
                _node = node;
                _actionScheduler = actionScheduler;
            }

            public void Handle(TimerFired message)
            {
                _actionScheduler.DoTimerFired();
            }

            public void Handle(NodeInitialised message)
            {
                _node.LogMessage(message);
                _node.OnInitialised();
            }

            public void Handle(TerminateNode message)
            {
                _node.Poller.Stop();
            }
        }
    }
}
