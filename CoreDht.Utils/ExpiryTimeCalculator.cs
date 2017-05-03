using System;

namespace CoreDht.Utils
{
    public class ExpiryTimeCalculator : IExpiryTimeCalculator
    {
        private readonly IUtcClock _clock;

        public ExpiryTimeCalculator(IUtcClock clock)
        {
            _clock = clock;
        }

        public DateTime CalcExpiry(int timeoutMilliSec)
        {
            return _clock.Now + new TimeSpan(0, 0, 0, 0, timeoutMilliSec);
        }
    }
}