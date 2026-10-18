using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// How far the empire has committed to a planet.
    /// </summary>
    public enum ColonyPresence
    {
        Outpost = 0,
        Colony = 1,
    }

    /// <summary>
    /// One building placed on a colony, upgraded in place.
    /// </summary>
    public sealed class ColonyBuildingState
    {
        public ContentId BuildingId { get; }

        public int Level { get; private set; }

        public ColonyBuildingState(ContentId buildingId, int level)
        {
            BuildingId = buildingId;
            Level = level < 1 ? 1 : level;
        }

        internal void IncreaseLevel()
        {
            Level++;
        }
    }

    /// <summary>
    /// Timeline-owned claim on one planet. Outposts do not count as colonies and do not produce.
    /// </summary>
    public sealed class ColonyState
    {
        private readonly Dictionary<ContentId, ColonyBuildingState> _buildings = new();

        public ContentId PlanetId { get; }

        public ColonyPresence Presence { get; private set; }

        public IReadOnlyDictionary<ContentId, ColonyBuildingState> Buildings => _buildings;

        public ColonyState(ContentId planetId, ColonyPresence presence)
        {
            PlanetId = planetId;
            Presence = presence;
        }

        internal void PromoteToColony()
        {
            Presence = ColonyPresence.Colony;
        }

        internal ColonyBuildingState FindBuilding(ContentId buildingId)
        {
            return _buildings.TryGetValue(buildingId, out var building) ? building : null;
        }

        internal void AddBuilding(ColonyBuildingState building)
        {
            if (building == null || building.BuildingId.IsEmpty)
            {
                return;
            }

            _buildings.TryAdd(building.BuildingId, building);
        }
    }
}
