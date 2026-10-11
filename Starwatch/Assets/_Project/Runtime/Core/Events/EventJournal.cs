using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Lightweight ordered log of domain events for the active run.
    /// Project-scoped for DI; RunManager clears it on CreateRun and ClearRun.
    /// </summary>
    public sealed class EventJournal
    {
        public IReadOnlyList<EventRecord> Records => _records;

        private readonly List<EventRecord> _records = new List<EventRecord>();

        /// <summary>
        /// Appends a structured event record when the event type is not None.
        /// </summary>
        public void Record(
            GameEventType eventType,
            GameTime simulationTime,
            EntityId sourceId,
            ContentId payloadId)
        {
            if (eventType == GameEventType.None)
            {
                return;
            }

            _records.Add(new EventRecord(eventType, simulationTime, sourceId, payloadId));
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
