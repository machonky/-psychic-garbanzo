using System;
using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// A Node joining a network transmits this message to any known node. The reciever replies or forwards a response to calculate 
    /// where in the network the applicant should insert itself.
    /// The correlationId is a new Guid to identify all network correspondence relating to the operation.
    /// </summary>
    public class JoinNetwork : NodeMessage, ICorrelatedMessage<Guid>
    {
        public Guid CorrelationId { get; }

        public JoinNetwork(NodeInfo recipient, Guid correlationId) : base(recipient)
        {
            CorrelationId = correlationId;
        }

        /// <summary>
        /// This is an internal message to ensure that this node catches the response from the Joinee.
        /// </summary>
        public class Await : Message, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }

            public Await(Guid correlationId)
            {
                CorrelationId = correlationId;
            }
        }

        /// <summary>
        /// A JoinNetwork.Reply is sent when a node identifies itself as a successor to a node transmitting a JoinNetwork message.
        /// The contained SuccessorList is the hash-ordered collection of distinct successors of the respondent
        /// </summary>
        public class Reply : NodeReply, ICorrelatedMessage<Guid>
        {
            public Guid CorrelationId { get; }
            public NodeInfo[] SuccessorList { get; }

            public Reply(NodeInfo sender, Guid correlationId, NodeInfo[] successorList) : base(sender)
            {
                CorrelationId = correlationId;
                SuccessorList = successorList;
            }
        }
    }
}