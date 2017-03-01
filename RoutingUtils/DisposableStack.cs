using System;
using System.Collections.Generic;

namespace CoreDht
{
    public class DisposableStack : IDisposable
    {
        private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

        public T Push<T>(T disposable) where T:IDisposable
        {
            _disposables.Push(disposable);
            return disposable;
        }

        #region IDisposable Support

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                while (_disposables.Count > 0)
                {
                    _disposables.Pop().Dispose();
                }
                isDisposed = true;
            }
        }
        #endregion
    }
}