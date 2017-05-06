using System;
using System.Collections.Generic;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;
using NetMQ;

namespace CoreDht.Node
{
    partial class Node
    {
        public class AwaitAckHandler : IDisposable
            , IHandle<AwaitMessage>
            , IHandle<AwaitWithTimeoutMessage>
            , IHandle<AckMessage>
        {
            private readonly IActionScheduler _actionScheduler;
            private readonly IExpiryTimeCalculator _expiryCalculator;
            private readonly INodeMarshaller _marshaller;
            private readonly IOutgoingSocket _actorSocket;
            private readonly Action<string> _logger;
            private readonly AwaitConfiguration _config;

            private readonly Dictionary<CorrelationId, DateTime> _acks = new Dictionary<CorrelationId, DateTime>();

            public AwaitAckHandler(IActionScheduler actionScheduler, IExpiryTimeCalculator expiryCalculator, INodeMarshaller marshaller, IOutgoingSocket actorSocket, Action<string> logger, AwaitConfiguration config)
            {
                _actionScheduler = actionScheduler;
                _disposeAction = new DisposableAction(
                    () => { _actionScheduler.ExecuteAction += OneExecuteAction; }, 
                    () => { _actionScheduler.ExecuteAction -= OneExecuteAction; });
                
                _expiryCalculator = expiryCalculator;
                _config = config;
                _marshaller = marshaller;
                _actorSocket = actorSocket;
                _logger = logger;
            }

            private void OneExecuteAction(object sender, ActionSchedulerEventArgs e)
            {
                // Decide if we should execute the action or reschedule to a new time.
                var context = e.State as Context;
                if (context != null)
                {
                    var correlation = context.CorrelationId;

                    DateTime newDueTime;
                    if (_acks.TryGetValue(correlation, out newDueTime))
                    {
                        e.RescheduleAt = newDueTime;
                        _acks.Remove(correlation);
                    }
                }
            }

            private class Context
            {
                public CorrelationId CorrelationId { get; set; }
            }

            public void Handle(AwaitMessage message)
            {
                var correlation = message.CorrelationId;
                var dueTime = _expiryCalculator.CalcExpiry(_config.AwaitTimeout);
                // If our await expires, we are no longer interested in any further action or timeout extensions.
                _actionScheduler.ScheduleAction(dueTime, new Context { CorrelationId = correlation },
                cxt =>
                {
                    _marshaller.Send(new CancelOperation(correlation), _actorSocket);
                    _acks.Remove(correlation);
                });

                _logger?.Invoke($"{message.GetType().Name} Id:{message.CorrelationId} ({_config.AwaitTimeout} ms)");
            }

            public void Handle(AwaitWithTimeoutMessage message)
            {
                var correlation = message.CorrelationId;
                var dueTime = _expiryCalculator.CalcExpiry(message.Timeout);

                _actionScheduler.ScheduleAction(dueTime, new Context { CorrelationId = correlation },
                cxt =>
                {
                    _marshaller.Send(new CancelOperation(correlation), _actorSocket);
                    _acks.Remove(correlation);
                });

                _logger?.Invoke($"{message.GetType().Name} Id:{message.CorrelationId} ({message.Timeout} ms)");

            }

            public void Handle(AckMessage message)
            {
                // Extend a timout while the sender of the Ack is processing the original request. 
                _logger?.Invoke($"AckMessage received Id:{message.CorrelationId}");

                var dueTime = _expiryCalculator.CalcExpiry(_config.AckTimeout);
                _acks[message.CorrelationId] = dueTime;
            }

            #region IDisposable Support

            private bool _isDisposed = false;
            private readonly DisposableAction _disposeAction;

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _disposeAction.Dispose();
                    _isDisposed = true;
                }
            }

            #endregion
        }
    }
}
