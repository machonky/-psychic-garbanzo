using System;
using System.Collections.Generic;

namespace CoreDht.Utils
{
    public class DisposableStack : IDisposable
    {
        private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

        public T Push<T>(T newObject)
        {
            if (newObject != null)
            {
                var disposable = newObject as IDisposable;
                if (disposable != null)
                {
                    _disposables.Push(disposable);
                }
            }
            return newObject;
        }

        #region IDisposable Support

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                while (_disposables.Count > 0)
                {
                    var disposable = _disposables.Pop();
                    disposable.Dispose();
                }
                isDisposed = true;
            }
        }
        #endregion
    }

    public class DisposableAction : IDisposable
    {
        private readonly Action _disposeAction;

        public DisposableAction(Action initAction, Action disposeAction)
        {
            initAction?.Invoke();
            _disposeAction = disposeAction;
        }
        public DisposableAction(Action disposeAction) : this(null, disposeAction)
        { }

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