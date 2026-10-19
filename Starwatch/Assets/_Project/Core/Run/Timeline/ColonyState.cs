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
        internal const int kMaxBuildOrders = 4;

        private readonly Dictionary<ContentId, ColonyBuildingState> _buildings = new();
        private readonly List<ShipBuildOrder> _buildQueue = new();

        public ContentId PlanetId { get; }

        public ColonyPresence Presence { get; private set; }

        public IReadOnlyDictionary<ContentId, ColonyBuildingState> Buildings => _buildings;

        public IReadOnlyList<ShipBuildOrder> BuildQueue => _buildQueue;

        public bool CanEnqueueBuild => _buildQueue.Count < kMaxBuildOrders;

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

        internal void EnqueueBuild(ShipBuildOrder order)
        {
            if (order == null || order.ClassId.IsEmpty)
            {
                return;
            }

            _buildQueue.Add(order);
        }

        internal bool TryTakeBuild(int index, out ShipBuildOrder order)
        {
            if (index < 0 || index >= _buildQueue.Count)
            {
                order = null;
                return false;
            }

            order = _buildQueue[index];
            _buildQueue.RemoveAt(index);
            return true;
        }
    }
}
