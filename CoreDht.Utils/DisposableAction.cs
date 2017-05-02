using System;

namespace CoreDht.Utils
{
    public class DisposableAction : IDisposable
    {
        private readonly Action _disposeAction;

        public DisposableAction(Action initAction, Action disposeAction)
        {
            initAction?.Invoke();
            _disposeAction = disposeAction;
        }
        public DisposableAction(Action disposeAction) :this(null, disposeAction)
        {}

        #region IDisposable Support

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _disposeAction();
                _isDisposed = true;
            }
        }

        #endregion
    }
}