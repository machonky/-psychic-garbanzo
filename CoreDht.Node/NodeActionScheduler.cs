using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using NetMQ;

namespace CoreDht.Node
{
    /// <summary>
    /// NodeActionScheduler ensures that timer events created by the ActionScheduler are enqueued with all other events via the actor
    /// so that even timer events occur in a single thread. As a consequence there is no contention on the scheduled action heap, so no locking 
    /// is required.
    /// </summary>
    public class NodeActionScheduler : ActionScheduler
    {
        private readonly INodeMarshaller _marshaller;
        private readonly IOutgoingSocket _actorSocket;

        public NodeActionScheduler(IUtcClock clock, IActionTimer timer, INodeMarshaller marshaller, IOutgoingSocket actorSocket) 
            : base(clock, timer, LockingStrategy.None)
        {
            _marshaller = marshaller;
            _actorSocket = actorSocket;
        }

        protected override void OnTimerFired()
        {
            _marshaller.Send(new TimerFired(), _actorSocket);
        }
    }
}