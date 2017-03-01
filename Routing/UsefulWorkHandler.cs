using System;
using CoreMemoryBus;

namespace Routing
{
    public class UsefulWorkHandler : IHandle<DoUsefulWork>
    {
        public void Handle(DoUsefulWork message)
        {
            Console.WriteLine(message.RoutingTarget);
            Console.WriteLine(message.Data);
        }
    }
}