using System;
using System.Threading;

namespace CoreDht.Utils
{
    public class ActionTimer : IActionTimer
    {
        private readonly Timer _systemTimer = new Timer(InvokeCallback, null, Timeout.Infinite, Timeout.Infinite);
        private static Action _callback;

        private static void InvokeCallback(object state)
        {
            _callback?.Invoke();
        }

        public void FireIn(int milliseconds, Action action)
        {
            _callback = action;
            var dueTime = milliseconds == Timeout.Infinite ? Timeout.Infinite : Math.Max(0, milliseconds);
            _systemTimer.Change(dueTime, Timeout.Infinite);
        }

        #region IDisposable Support

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _systemTimer.Dispose();
                _isDisposed = true;
            }
        }

        #endregion
    }
}