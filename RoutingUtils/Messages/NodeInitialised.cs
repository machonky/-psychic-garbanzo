using CoreMemoryBus.Messages;

namespace CoreDht
{
    /// <summary>
    /// Derived node Applications can subscrive to this event when the node is ready to process application messages.
    /// </summary>
    public class NodeInitialised : Message
    {}
}