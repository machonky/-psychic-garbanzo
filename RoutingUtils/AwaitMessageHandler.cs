using CoreMemoryBus;

namespace CoreDht
{
    public class AwaitMessageHandler : IHandle<AwaitMessage>
    {
        public Node Node { get; }

        public AwaitMessageHandler(Node node)
        {
            Node = node;
        }

        public void Handle(AwaitMessage message)
        {
            // TODO: 
            // When an await message is sent we can create a timer, 
            // which on expiry will close the state object waiting for a reply - creating 
            // a simple timeout. This is done by sending a CancelOperation/Timeout message with 
            // the correlation in the AwaitMessage.
        }
    }
}