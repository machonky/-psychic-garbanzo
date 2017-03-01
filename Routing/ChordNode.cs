using CoreDht;
using CoreMemoryBus.Messaging;
using Routing.Messages;

namespace Routing
{
    public partial class ChordNode : IPublisher<RoutableMessage>
    {
        private readonly UsefulBoundedContextRepository _dataNodes = new UsefulBoundedContextRepository();

        private static readonly IConsistentHashingService _hashingService = new Sha1HashingService();

        public ChordNodeInfo Identity { get; private set; }
        public ConsistentHash NodeKey { get { return Identity.NodeKey; } }

        private readonly FingerTable _fingerTable;

        public ChordNode(string identifier)
        {
            Identity = new ChordNodeInfo(identifier, GetHash(identifier));
            _messageBus.Subscribe(new NetworkingMessageHandler(this));
            _fingerTable = new FingerTable(Identity);
        }

        public static ConsistentHash GetHash(string identifier)
        {
            return _hashingService.GetConsistentHash(identifier);
        }

        readonly MemoryBus _messageBus = new MemoryBus();

        public void TryPublish(RoutableMessage message)
        {
            if (IsInDomain(message.RoutingTarget))
            {
                _dataNodes.TryPublish(message);
            }

            _messageBus.Publish(message);
        }

        private bool IsInDomain(ConsistentHash routingHash)
        {
            return routingHash.Equals(Identity.NodeKey);
        }

        public override string ToString()
        {
            return Identity.ToString();
        }

        public static bool IsIDInRange(ConsistentHash id, ConsistentHash start, ConsistentHash end)
        {
            if (start < end)
            {
                if (id > start && id <= end)
                {
                    return true;
                }
            }
            else //wraparound
            {
                if (id > start || id <= end)
                {
                    return true;
                }
            }

            return false;
        }

        public FindSuccessor EmitFindSuccessor(ConsistentHash routingTarget)
        {
            return new FindSuccessor(routingTarget, Identity.NodeKey,
                new PublisherReplyEnvelope<FindSuccessor.Result>(_messageBus));
        }

        public JoinNetwork EmitJoinNetwork(ConsistentHash routingTarget)
        {
            return new JoinNetwork(routingTarget) {ApplicantInfo = Identity.Clone() };
        }
    }
}
