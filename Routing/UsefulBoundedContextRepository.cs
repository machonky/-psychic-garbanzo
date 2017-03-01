using CoreDht;

namespace Routing
{
    public class UsefulBoundedContextRepository : RoutableRepository<ConsistentHash, UsefulBoundedContext>
    {
        public UsefulBoundedContextRepository()
            : base(message => new UsefulBoundedContext(((RoutableMessage)message).RoutingTarget))
        { }
    }
}