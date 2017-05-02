using System;

namespace CoreDht.Utils
{
    public interface IActionScheduler
    {
        void Stop();
        void ScheduleAction(DateTime dueTime, object actionState, Action<object> action);

        event EventHandler<ActionSchedulerEventArgs> ExecuteAction;
        void DoTimerFired();
    }

    public class ActionSchedulerEventArgs : EventArgs
    {
        public DateTime? RescheduleAt { get; set; }
        public object State { get; set; }
    }
}
