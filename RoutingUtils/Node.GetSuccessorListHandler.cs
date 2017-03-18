using System;

namespace CoreDht
{
    partial class Node
    {
        public class GetSuccessorListHandler : StateHandler
        {
            public GetSuccessorListHandler(Guid correlationId, Node node) :base(correlationId, node)
            {
            }
        }
    }
}