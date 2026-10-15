using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Core.Events
{
    /// <summary>
    /// Whether a fleet leg has departed or reached its destination.
    /// </summary>
    public enum FleetTravelKind
    {
        Started = 0,
        Arrived = 1,
    }

    /// <summary>
    /// Whether map knowledge gained a node or a planet scan.
    /// </summary>
    public enum MapKnowledgeKind
    {
        NodeExplored = 0,
        PlanetScanned = 1,
    }

    /// <summary>
    /// A fleet departed along a known route, or arrived at the destination.
    /// </summary>
    public readonly struct FleetTravelEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public EntityId FleetId { get; }

        public ContentId FromNodeId { get; }

        public ContentId ToNodeId { get; }

        public FleetTravelKind Kind { get; }

        public GameEventType EventType => Kind == FleetTravelKind.Arrived
            ? GameEventType.FleetArrived
            : GameEventType.FleetTravelStarted;

        public FleetTravelEvent(
            GameTime time,
            EntityId fleetId,
            ContentId fromNodeId,
            ContentId toNodeId,
            FleetTravelKind kind)
        {
            Time = time;
            FleetId = fleetId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Kind = kind;
        }
    }

    /// <summary>
    /// An observer explored a node or scanned a planet.
    /// </summary>
    public readonly struct MapKnowledgeChangedEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public ObserverId ObserverId { get; }

        public ContentId SubjectId { get; }

        public MapKnowledgeKind Kind { get; }

        public GameEventType EventType => Kind == MapKnowledgeKind.PlanetScanned
            ? GameEventType.PlanetScanned
            : GameEventType.NodeExplored;

        public MapKnowledgeChangedEvent(
            GameTime time,
            ObserverId observerId,
            ContentId subjectId,
            MapKnowledgeKind kind)
        {
            Time = time;
            ObserverId = observerId;
            SubjectId = subjectId;
            Kind = kind;
        }
    }
}
