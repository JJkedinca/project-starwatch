using System.Collections.Generic;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Ordered log of domain events for the active run.
    /// Project-scoped for DI; RunManager clears it on CreateRun and ClearRun.
    /// </summary>
    internal sealed class EventJournal
    {
        public IReadOnlyList<EventRecord> Records => _records;

        private readonly List<EventRecord> _records = new();

        /// <summary>
        /// Appends the domain event when its type is not None.
        /// </summary>
        public void Record<T>(T domainEvent)
            where T : struct, IDomainEvent
        {
            if (domainEvent.EventType == GameEventType.None)
            {
                return;
            }

            _records.Add(new EventRecord(domainEvent));
        }

        /// <summary>
        /// Clears all recorded events.
        /// </summary>
        public void Clear()
        {
            _records.Clear();
        }
    }
}
