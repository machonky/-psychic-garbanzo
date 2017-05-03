using System;

namespace CoreDht.Utils
{
    public class UtcClock : IUtcClock
    {
        public DateTime Now => DateTime.UtcNow;
    }
}