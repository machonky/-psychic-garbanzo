using System;
using CoreDht.Utils;

namespace CoreDht
{
    public class GuidCorrelationFactory : ICorrelationFactory<CorrelationId>
    {
        public CorrelationId GetNextCorrelation()
        {
            return CorrelationId.NewId();
        }
    }
}