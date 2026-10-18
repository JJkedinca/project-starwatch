using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Archive
{
    /// <summary>
    /// Persistent map discoveries for one observer. Survives timeline rewind.
    /// </summary>
    public sealed class MapKnowledge
    {
        public ObserverId ObserverId { get; }

        public IReadOnlyCollection<ContentId> ExploredNodeIds => _exploredNodeIds;

        public IReadOnlyCollection<ContentId> KnownRouteKeys => _knownRouteKeys;

        public IReadOnlyCollection<ContentId> ScannedPlanetIds => _scannedPlanetIds;

        private readonly HashSet<ContentId> _exploredNodeIds = new();
        private readonly HashSet<ContentId> _knownRouteKeys = new();
        private readonly HashSet<ContentId> _scannedPlanetIds = new();

        public MapKnowledge(ObserverId observerId)
        {
            ObserverId = observerId.IsEmpty ? ObserverId.Player : observerId;
        }

        /// <summary>
        /// Adds an explored node when non-empty. Returns true only when newly added.
        /// </summary>
        public bool TryAddExploredNode(ContentId nodeId)
        {
            return !nodeId.IsEmpty && _exploredNodeIds.Add(nodeId);
        }

        /// <summary>
        /// Adds a known route key when non-empty. Returns true only when newly added.
        /// </summary>
        public bool TryAddKnownRoute(ContentId routeKey)
        {
            return !routeKey.IsEmpty && _knownRouteKeys.Add(routeKey);
        }

        /// <summary>
        /// Adds a scanned planet when non-empty. Returns true only when newly added.
        /// </summary>
        public bool TryAddScannedPlanet(ContentId planetId)
        {
            return !planetId.IsEmpty && _scannedPlanetIds.Add(planetId);
        }

        /// <summary>
        /// Returns whether the node has been explored.
        /// </summary>
        public bool HasExploredNode(ContentId nodeId)
        {
            return !nodeId.IsEmpty && _exploredNodeIds.Contains(nodeId);
        }

        /// <summary>
        /// Returns whether the undirected route key is known.
        /// </summary>
        public bool HasKnownRoute(ContentId routeKey)
        {
            return !routeKey.IsEmpty && _knownRouteKeys.Contains(routeKey);
        }

        /// <summary>
        /// Returns whether the planet has been scanned.
        /// </summary>
        public bool HasScannedPlanet(ContentId planetId)
        {
            return !planetId.IsEmpty && _scannedPlanetIds.Contains(planetId);
        }
    }
}
