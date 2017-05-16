using System;
using System.Threading;

namespace CoreDht.Utils
{
    /// <summary>
    /// Based upon EventStore Scheduler. 
    /// <br>
    /// </summary>
    public class ActionScheduler : IActionScheduler, IDisposable
    {
        protected IUtcClock Clock { get; }
        private readonly IActionTimer _timer;
        private readonly PairingHeap<ScheduledAction> _actions = new PairingHeap<ScheduledAction>((x, y) => x.DueTime < y.DueTime);
        private readonly object _actionsToken = new object();

        public event EventHandler<ActionSchedulerEventArgs> ExecuteAction;

        #region Various locking strategies

        protected interface ILockingStrategy : IDisposable
        {
            void Lock();
            void Unlock();
        }
        
        protected struct DefaultLockingStrategy : ILockingStrategy
        {
            private bool _isDisposed;
            private readonly object _token;

            public DefaultLockingStrategy(object token)
            {
                _isDisposed = false;
                _token = token;
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    Unlock();
                    _isDisposed = true;
                }
            }

            public void Lock()
            {
                Monitor.Enter(_token);
            }

            public void Unlock()
            {
                Monitor.Exit(_token);
            }
        }

        #endregion

        public ActionScheduler(IUtcClock clock, IActionTimer timer)
        {
            Clock = clock;
            _timer = timer;
        }

        public void Stop()
        {
            Dispose();
        }

        protected virtual ILockingStrategy NewLock()
        {
            return new DefaultLockingStrategy(_actionsToken);
        }

        public void ScheduleAction(DateTime dueTime, object actionState, Action<object> action)
        {
            using (NewLock())
            {
                _actions.Add(new ScheduledAction(dueTime, actionState, action));
                ResetTimer();
            }
        }

        private void ResetTimer()
        {
            if (_actions.Count > 0)
            {
                var action = _actions.FindMin();
                _timer.FireIn((int)(action.DueTime - Clock.Now).TotalMilliseconds, OnTimerFired);
            }
            else
            {
                _timer.FireIn(Timeout.Infinite, OnTimerFired);
            }
        }

        protected virtual void OnTimerFired()
        {
            DoTimerFired();
        }

        public void DoTimerFired()
        {
            using (NewLock())
            {
                ProcessOperations();
                ResetTimer();
            }
        }

        private void ProcessOperations()
        {
            while (_actions.Count > 0 && _actions.FindMin().DueTime <= Clock.Now)
            {
                var scheduledTask = _actions.DeleteMin();

                // Check if we should fire the action.
                var args = new ActionSchedulerEventArgs();
                args.State = scheduledTask.State;

                ExecuteAction?.Invoke(this, args);

                if (!args.RescheduleAt.HasValue)
                {
                    scheduledTask.Action(scheduledTask.State);
                }
                else // Reschedule
                {
                    ScheduleAction(args.RescheduleAt.Value, scheduledTask.State, scheduledTask.Action);
                }
            }
        }

        private struct ScheduledAction
        {
            public ScheduledAction(DateTime dueTime, object state, Action<object> action)
            {
                DueTime = dueTime;
                State = state;
                Action = action;
            }

            public DateTime DueTime { get; }
            public object State { get; }
            public Action<object> Action { get; } 
        }

        #region IDisposable Support
        private bool _isDisposed = false;

        // Derived classes may also have disposable resources so we have to do this hack!
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}