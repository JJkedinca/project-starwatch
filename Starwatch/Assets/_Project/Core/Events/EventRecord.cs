using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Immutable journal entry for a domain event.
    /// </summary>
    public readonly struct EventRecord
    {
        public GameEventType EventType { get; }

        public GameTime SimulationTime { get; }

        public EntityId SourceId { get; }

        public ContentId PayloadId { get; }

        public EventRecord(
            GameEventType eventType,
            GameTime simulationTime,
            EntityId sourceId,
            ContentId payloadId)
        {
            EventType = eventType;
            SimulationTime = simulationTime;
            SourceId = sourceId;
            PayloadId = payloadId;
        }
    }
}
