using System;

namespace CoreDht.Utils
{
    public class ExpiryTimeCalculator : IExpiryTimeCalculator
    {
        private readonly IClock _clock;

        public ExpiryTimeCalculator(IClock clock)
        {
            _clock = clock;
        }

        public DateTime CalcExpiry(int timeoutMilliSec)
        {
            return _clock.Now + new TimeSpan(0, 0, 0, 0, timeoutMilliSec);
        }
    }
}