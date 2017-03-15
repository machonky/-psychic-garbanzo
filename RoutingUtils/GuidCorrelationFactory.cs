using System;

namespace CoreDht
{
    public class GuidCorrelationFactory : ICorrelationFactory<Guid>
    {
        public Guid GetNextCorrelation()
        {
            return Guid.NewGuid();
        }
    }
}