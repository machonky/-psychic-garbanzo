using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
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
            private readonly ICommunicationManager _commMgr;

            public StabilizeHandler(Node node, ICommunicationManager commMgr)
            {
                _node = node;
                _commMgr = commMgr;
            }

            public void Handle(InitStabilize message)
            {
                _node.LogMessage(message);
                ScheduleNextStabilize();
                Stabilize(_node.Successor);
            }

            private void Stabilize(NodeInfo successorInfo)
            {
                // Send a stabilize message to my successor to learn it's predecessor
                //   Stabilize might need to be retried as other nodes will be joining at the same location as it is a seed node.

                var opId = _node.Config.CorrelationFactory.GetNextCorrelation();
                var messageHandler = _node.CreateAwaitAllResponsesHandler();
                messageHandler
                    .PerformAction(() =>
                    {
                        var msg = new Stabilize(_node.Identity, successorInfo, opId);
                        _node.Log($"Sending {msg.TypeName()} To:{msg.To} Id:{opId}");
                        _commMgr.Send(msg);
                    })
                    .AndAwait(opId, (StabilizeReply reply) =>
                    {
                        var thisHash = _node.Identity.RoutingHash;
                        var predecessorHash = reply.Predecessor.RoutingHash;
                        var successorHash = successorInfo.RoutingHash;

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
                            StabilizeFromPredecessor(reply);
                        }
                    })
                    .Run(opId);
            }

            private void StabilizeFromSuccessor(StabilizeReply reply)
            {
                _node.Log($"StabilizeFromSuccessor");
                _node.Predecessor = reply.Predecessor;
                _node.SuccessorTable.Copy(reply.SuccessorTableEntries);

                _commMgr.SendInternal(new InitRectify());
            }

            private void StabilizeFromPredecessor(StabilizeReply reply)
            {
                _node.Log($"StabilizeFromPredecessor");
                // The predecessor is a better successor than the current one.
                // We should quit this operation and stabilize against the precedessor
                _commMgr.SendInternal(new CancelOperation(reply.CorrelationId));
                Stabilize(reply.Predecessor);
                //_commMgr.SendInternal(new JoinToSeed {SeedNode = reply.Predecessor}); // this causes an infinite loop
            }

            private void ScheduleNextStabilize()
            {
                // Schedule the next stabilize operation and prevent self similar behaviour
                int min = _node.Config.StabilizeSettings.StabilizeMinInterval;
                int max = _node.Config.StabilizeSettings.StabilizeMaxInterval;
                var interval = _node.Config.Random.Next(min, max);
#if DEBUG
                interval = int.MaxValue;
#endif

                var dueTime = _node.ExpiryCalculator.CalcExpiry(interval);
                _node.Log($"ScheduleNextStabilize Due:{dueTime}");
                _node.ActionScheduler.ScheduleAction(dueTime, null, _ => _commMgr.SendInternal(new InitStabilize()));
            }

            public void Handle(Stabilize message)
            {
                _node.LogMessage(message);
                _commMgr.SendAck(message, message.CorrelationId);

                var reply = new StabilizeReply(_node.Identity, message.From, message.CorrelationId)
                {
                    Predecessor = _node.Predecessor,
                    // Take the opportunity to ensure the successor table is up to date.
                    SuccessorTableEntries = _node.SuccessorTable.Entries,
                };
                _node.Log($"Sending {reply.TypeName()} Id:{reply.CorrelationId}");
                _commMgr.Send(reply);
            }
        }
    }
}
