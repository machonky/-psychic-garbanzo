using System;
using CoreDht.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    /// <summary>
    /// This wrapper records if there were errors when attempting to send, 
    /// so we don't need to test the socket by sending again.
    /// </summary>
    public class OutgoingSocket : IDisposable, IOutgoingSocket
    {
        private readonly IUtcClock _clock;
        public IOutgoingSocket Socket { get; }
        public bool Error { get; private set; }

        public DateTime LastTransmission { get; private set; }

        public OutgoingSocket(DealerSocket socket, IUtcClock clock)
        {
            _clock = clock;
            Socket = socket;
        }

        public OutgoingSocket(NetMQActor socket, IUtcClock clock)
        {
            _clock = clock;
            Socket = socket;
            _isDisposed = true; // Actor is not owned by the cache
        }

        public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
        {
            try
            {
                // If we record the time, we can potentially exclude this socket from a heartbeat 
                // if it took part in a recent transmission to save bandwidth
                LastTransmission = _clock.Now; 
                return Socket.TrySend(ref msg, timeout, more);
            }
            catch (Exception e) // Change this to specific type
            {
                Error = true;
                throw e; // Instead of throwing we can publish the error - but to which handler? Msg might have an answer, but we should not want to keep the reference if we want to pool the messages.
            }
        }

        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (!_isDisposed)
            {
                var disposable = Socket as IDisposable;
                disposable?.Dispose();
                _isDisposed = true;
            }
        }

        #endregion
    }
}