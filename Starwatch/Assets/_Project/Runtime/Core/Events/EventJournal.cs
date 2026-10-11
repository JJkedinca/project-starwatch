using System.Collections.Generic;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Lightweight ordered log of domain events for debugging, telemetry, and tests.
    /// </summary>
    public sealed class EventJournal
    {
        private readonly List<string> _records = new List<string>();

        public IReadOnlyList<string> Records => _records;

        /// <summary>
        /// Appends an event type when the value is non-empty.
        /// </summary>
        public void Record(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return;
            }

            _records.Add(eventType);
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
