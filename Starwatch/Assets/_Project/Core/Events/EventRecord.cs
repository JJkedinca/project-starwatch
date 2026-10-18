using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Core.Events
{
    /// <summary>
    /// One journaled domain event. The payload is the event that was published.
    /// </summary>
    public readonly struct EventRecord
    {
        public GameEventType EventType { get; }

        public GameTime SimulationTime { get; }

        public object Event { get; }

        public EventRecord(IDomainEvent domainEvent)
        {
            EventType = domainEvent.EventType;
            SimulationTime = domainEvent.Time;
            Event = domainEvent;
        }

        /// <summary>
        /// Returns the stored event when it is the requested domain event type.
        /// </summary>
        public bool TryGet<T>(out T domainEvent)
            where T : struct, IDomainEvent
        {
            if (Event is T typed)
            {
                domainEvent = typed;
                return true;
            }

            domainEvent = default;
            return false;
        }
    }
}
