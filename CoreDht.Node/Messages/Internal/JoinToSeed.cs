using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.Internal
{
    public class JoinToSeed : Message
    {
        public NodeInfo SeedNode { get; set; }
    }
}