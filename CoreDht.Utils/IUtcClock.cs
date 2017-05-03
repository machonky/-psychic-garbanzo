using System;

namespace CoreDht.Utils
{
    public interface IUtcClock
    {
        DateTime Now { get; }
    }
}