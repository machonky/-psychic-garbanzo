using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class RectifyHandler
            : IHandle<InitRectify>
            , IHandle<Rectify>
            //, IHandle<RectifyReply>
        {
            private readonly Node _node;
            private readonly ICommunicationManager _commMgr;

            public RectifyHandler(Node node, ICommunicationManager commMgr)
            {
                _node = node;
                _commMgr = commMgr;
            }

            public void Handle(InitRectify message)
            {
                _node.LogMessage(message);

                // Notify the successor to update it's predecessor with this node's info
                var opId = _node.Config.CorrelationFactory.GetNextCorrelation();
                var handler =_node.CreateAwaitAllResponsesHandler();
                handler
                    .PerformAction(() =>
                    {
                        _node.Log($"Sending Rectify to {_node.Successor} Id:{opId}");
                        var msg = new Rectify(_node.Identity, _node.Successor, opId)
                        {
                            Predecessor = _node.Identity
                        };
                        _commMgr.Send(msg);
                    })
                    .AndAwait(opId, (RectifyReply reply) =>
                    {
                        _node.LogMessage(reply);
                        _node.Log($"{_node.Identity} Joined Network");
                    })
                    .Run(opId);
            }

            public void Handle(Rectify message)
            {
                _node.LogMessage(message);

                // Assign this node's predecessor to the value supplied.
                var oldPredecessor = _node.Predecessor;
                _node.Predecessor = message.Predecessor;
                _node.Log($"Rectify Assigned predecessor:{message.Predecessor}");

                var opId = _node.Config.CorrelationFactory.GetNextCorrelation();
                var handler = _node.CreateAwaitAllResponsesHandler();
                handler
                    .PerformAction(() =>
                    {
                        // notify the old predecessor to join to the new predecessor
                        _node.Log($"Notify {oldPredecessor} Id:{opId}");
                        var msg = new Notify(_node.Identity, oldPredecessor, opId)
                        {
                            NewSuccessor = message.Predecessor,
                        };
                        _node.Log($"Sending {msg.TypeName()} To {msg.To} Id:{opId}");
                        _commMgr.Send(msg);
                    })
                    .AndAwait(opId, (NotifyReply notifyReply) =>
                    {
                        _node.Log($"NotifyReply Id:{opId}");
                    })
                    .Run(opId);

                var reply = new RectifyReply(_node.Identity, message.From, message.CorrelationId);
                _commMgr.Send(reply);
            }
        }
    }
}
