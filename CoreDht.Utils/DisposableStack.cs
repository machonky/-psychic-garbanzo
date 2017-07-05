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
}