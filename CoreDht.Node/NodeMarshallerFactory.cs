namespace CoreDht.Node
{
    public class NodeMarshallerFactory : INodeMarshallerFactory
    {
        private readonly IMessageSerializer _serializer;

        public NodeMarshallerFactory(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public INodeMarshaller Create()
        {
            return new NodeMarshaller(_serializer);
        }
    }
}