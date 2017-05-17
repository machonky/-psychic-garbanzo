using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils.Hashing;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class StabilizeHandler
            : IHandle<InitStabilize>
            , IHandle<Stabilize>
        {
            private readonly Node _node;

            public StabilizeHandler(Node node)
            {
                _node = node;
            }

            public void Handle(InitStabilize message)
            {
                ScheduleNextStabilize();

                // Send a stabilize message to my successor to learn it's predecessor
                //   Stabilize might need to be retried as other nodes will be joining at the same location as it is a seed node.

                var opId = _node.Config.CorrelationFactory.GetNextCorrelation();
                var messageHandler = _node.CreateAwaitAllResponsesHandler();
                messageHandler
                    .PerformAction(() =>
                    {
                        var msg = new Stabilize(_node.Identity, _node.Successor, opId);
                        var forwardingSocket =_node.ForwardingSockets[_node.Successor.HostAndPort];
                        _node.Marshaller.Send(msg, forwardingSocket);
                    })
                    .AndAwait(opId, (StabilizeReply reply) =>
                    {
                        var thisHash = _node.Identity.RoutingHash;
                        var predecessorHash = reply.Predecessor.RoutingHash;
                        var successorHash = _node.Successor.RoutingHash;

                        // We need to check the order of the predecessor or the successor before adopting a value
                        // as nodes are racing to join the seed node - our last query may be out of date, but not by much
                        if (thisHash.IsBetween(predecessorHash, successorHash))
                        {
                            // No ordering change. We can adopt the predecessor values and tell the successor to rectify
                            StabilizeFromSuccessor(reply);
                        }
                        else if (predecessorHash.IsBetween(thisHash, successorHash))
                        {
                            // another node beat this one to join to the successor, changing the ordering...
                            StabilizeFromPredecessor();
                        }
                    })
                    .Run(opId);
            }

            private void StabilizeFromSuccessor(StabilizeReply reply)
            {
                _node.Predecessor = reply.Predecessor;
                _node.SuccessorTable.Copy(reply.SuccessorTableEntries);

                var nextMsg = new InitRectify();
                var forwardingSocket = _node.ForwardingSockets[_node.Identity.HostAndPort];
                _node.Marshaller.Send(nextMsg, forwardingSocket);
            }

            private void StabilizeFromPredecessor()
            {
                // StabilizeFromPredecessor...
                // The predecessor is a better successor than the current one.
                // We should quit this operation and stabilize against the precedessor
            }

            private void ScheduleNextStabilize()
            {
                // Schedule the next stabilize operation and prevent self similar behaviour
                int min = _node.Config.StabilizeSettings.StabilizeMinInterval;
                int max = _node.Config.StabilizeSettings.StabilizeMaxInterval;
                var interval = _node.Config.Random.Next(min, max);

                var dueTime = _node.ExpiryCalculator.CalcExpiry(interval);
                _node.ActionScheduler.ScheduleAction(dueTime, null, _ => _node.InitStabilize());
            }

            public void Handle(Stabilize message)
            {
                var reply = new StabilizeReply(_node.Identity, message.From, message.CorrelationId)
                {
                    Predecessor = _node.Predecessor,
                    SuccessorTableEntries = _node.SuccessorTable.Entries,
                    // Take the opportunity to ensure the successor table is up to date.
                };

                var forwardingSocket = _node.ForwardingSockets[message.From.HostAndPort];
                _node.Marshaller.Send(reply, forwardingSocket);
            }
        }
    }
}
