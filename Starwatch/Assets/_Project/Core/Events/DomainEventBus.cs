using System;
using System.Collections.Generic;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Domain event bus. Services publish after they commit state. The bus appends
    /// the journal, then notifies subscribers synchronously on this call.
    /// </summary>
    internal sealed class DomainEventBus
    {
        private readonly EventJournal _journal;
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public DomainEventBus(EventJournal journal)
        {
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
        }

        /// <summary>
        /// Records the event and notifies current subscribers.
        /// </summary>
        public void Publish<T>(T domainEvent)
            where T : struct, IDomainEvent
        {
            _journal.Record(domainEvent);

            if (!_handlers.TryGetValue(typeof(T), out var handlers) || handlers.Count == 0)
            {
                return;
            }

            var snapshot = new Delegate[handlers.Count];
            handlers.CopyTo(snapshot);
            for (var i = 0; i < snapshot.Length; i++)
            {
                ((Action<T>)snapshot[i]).Invoke(domainEvent);
            }
        }

        /// <summary>
        /// Registers a handler until the returned subscription is disposed.
        /// </summary>
        public IDisposable Subscribe<T>(Action<T> handler)
            where T : struct, IDomainEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.TryGetValue(typeof(T), out var handlers))
            {
                _handlers.Add(typeof(T), handlers = new List<Delegate>());
            }

            handlers.Add(handler);
            return new Subscription(this, typeof(T), handler);
        }

        private void Unsubscribe(Type eventType, Delegate handler)
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private DomainEventBus _bus;
            private readonly Type _eventType;
            private readonly Delegate _handler;

            public Subscription(DomainEventBus bus, Type eventType, Delegate handler)
            {
                _bus = bus;
                _eventType = eventType;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_bus == null)
                {
                    return;
                }

                _bus.Unsubscribe(_eventType, _handler);
                _bus = null;
            }
        }
    }
}
