using System;
using CoreDht;
using CoreMemoryBus;

namespace Routing
{
    public class UsefulBoundedContext :
        RepositoryItem<ConsistentHash, UsefulBoundedContext>,
        IAmTriggeredBy<StartUsefulWork>,
        IHandle<DoUsefulWork>
    {
        public UsefulBoundedContext(ConsistentHash routingHash) : base(routingHash)
        { }

        public void Handle(StartUsefulWork message)
        {
            Owner = message.Owner;
            Console.WriteLine("UsefulBoundedContext created for {0}", message.Owner);
        }

        public string Owner { get; private set; }

        public void Handle(DoUsefulWork message)
        {
            Console.WriteLine(message.Data);
        }
    }
}