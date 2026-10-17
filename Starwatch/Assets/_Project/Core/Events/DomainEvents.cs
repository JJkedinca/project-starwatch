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

    /// <summary>
    /// Whether the new claim is a cheap outpost or a colony.
    /// </summary>
    public enum SettlementKind
    {
        Outpost = 0,
        Colony = 1,
    }

    /// <summary>
    /// An outpost was placed or a colony was founded.
    /// </summary>
    public readonly struct SettlementFoundedEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public ContentId PlanetId { get; }

        public SettlementKind Kind { get; }

        public GameEventType EventType => GameEventType.SettlementFounded;

        public SettlementFoundedEvent(GameTime time, ContentId planetId, SettlementKind kind)
        {
            Time = time;
            PlanetId = planetId;
            Kind = kind;
        }
    }

    /// <summary>
    /// An outpost or colony was removed. The founding cost stays spent.
    /// </summary>
    public readonly struct SettlementAbandonedEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public ContentId PlanetId { get; }

        public SettlementKind Kind { get; }

        public GameEventType EventType => GameEventType.SettlementAbandoned;

        public SettlementAbandonedEvent(GameTime time, ContentId planetId, SettlementKind kind)
        {
            Time = time;
            PlanetId = planetId;
            Kind = kind;
        }
    }

    /// <summary>
    /// A building was placed or upgraded on a colony.
    /// </summary>
    public readonly struct BuildingChangedEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public ContentId PlanetId { get; }

        public ContentId BuildingId { get; }

        public int Level { get; }

        public GameEventType EventType => GameEventType.BuildingChanged;

        public BuildingChangedEvent(GameTime time, ContentId planetId, ContentId buildingId, int level)
        {
            Time = time;
            PlanetId = planetId;
            BuildingId = buildingId;
            Level = level;
        }
    }

    /// <summary>
    /// The timeline ledger changed after a committed economy tick.
    /// </summary>
    public readonly struct ResourceChangedEvent : IDomainEvent
    {
        public GameTime Time { get; }

        public GameEventType EventType => GameEventType.ResourceChanged;

        public ResourceChangedEvent(GameTime time)
        {
            Time = time;
        }
    }
}
