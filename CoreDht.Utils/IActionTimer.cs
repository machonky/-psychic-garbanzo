using System;

namespace CoreDht.Utils
{
    public interface IActionTimer: IDisposable
    {
        void FireIn(int milliseconds, Action action);
    }
}
