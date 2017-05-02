using System;

namespace CoreDht.Utils
{
    public class Clock : IClock
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}