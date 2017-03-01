using System;
using CoreMemoryBus;
using Routing.Messages;

namespace Routing
{
    public partial class ChordNode
    {
        public class NetworkingMessageHandler : 
            IHandle<JoinNetwork>, 
            IHandle<FindSuccessor>
        {
            public ChordNode ChordNode { get; private set; }

            public NetworkingMessageHandler(ChordNode chordNode)
            {
                ChordNode = chordNode;
            }

            public void Handle(JoinNetwork message)
            {
                Console.WriteLine($"Node:{message.ApplicantInfo.Identifier} wants to join the network via Node:{ChordNode.Identity.Identifier}");
                // We need to identify the successor and predecessor of the applicant
                if (ChordNode.IsIDInRange(message.ApplicantInfo.NodeKey, ChordNode.Predecessor.NodeKey,
                    ChordNode.NodeKey))
                {
                    
                }
            }

            public void Handle(FindSuccessor message)
            {
                message.Send.Reply(new FindSuccessor.Result(message.ReplyHash) {Successor = ChordNode.Identity.Clone()});
            }
        }
    }
}