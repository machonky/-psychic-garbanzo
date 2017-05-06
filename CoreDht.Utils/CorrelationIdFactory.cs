using System;
using CoreDht.Utils;

namespace CoreDht
{
    public class CorrelationIdFactory : ICorrelationFactory<CorrelationId>
    {
        public CorrelationId GetNextCorrelation()
        {
            return CorrelationId.NewId();
        }
    }
}