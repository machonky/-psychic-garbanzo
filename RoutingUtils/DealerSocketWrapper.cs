using System;
using CoreDht.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht
{
    /// <summary>
    /// This wrapper records if there were errors when attempting to send, 
    /// so we don't need to test the socket by sending again.
    /// </summary>
    public class DealerSocketWrapper : IDisposable, IOutgoingSocket
    {
        private readonly IUtcClock _clock;
        public DealerSocket Socket { get; }
        public bool Error { get; private set; }

        public DateTime LastTransmission { get; private set; }

        public DealerSocketWrapper(DealerSocket socket, IUtcClock clock)
        {
            _clock = clock;
            Socket = socket;
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
                throw e;
            }
        }

        #region IDisposable Support

        private bool isDisposed = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (!isDisposed)
            {
                Socket.Dispose();
                isDisposed = true;
            }
        }

        #endregion
    }
}