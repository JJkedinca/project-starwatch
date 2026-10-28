using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Overworld;

namespace ProjectStarwatch.Overworld.Services
{
    /// <summary>
    /// Fog / explored / scanned queries over archive knowledge and timeline presence.
    /// </summary>
    internal sealed class VisibilityService
    {
        /// <summary>
        /// Returns visibility for a map node for the given observer.
        /// </summary>
        public MapNodeVisibility GetNodeVisibility(
            RunArchive archive,
            TimelineState timeline,
            ObserverId observerId,
            ContentId nodeId)
        {
            if (nodeId.IsEmpty)
            {
                return MapNodeVisibility.Hidden;
            }

            if (timeline != null && timeline.Fleets.HasFleetAt(nodeId))
            {
                return MapNodeVisibility.Present;
            }

#if STARWATCH_DEBUG
            return MapNodeVisibility.Explored;
#else
            if (archive == null || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return MapNodeVisibility.Hidden;
            }

            return knowledge.HasExploredNode(nodeId)
                ? MapNodeVisibility.Explored
                : MapNodeVisibility.Hidden;
#endif
        }

        /// <summary>
        /// Returns visibility for a planet given its host system.
        /// </summary>
        public PlanetVisibility GetPlanetVisibility(
            RunArchive archive,
            ObserverId observerId,
            ContentId systemId,
            ContentId planetId)
        {
            if (planetId.IsEmpty)
            {
                return PlanetVisibility.Hidden;
            }

#if STARWATCH_DEBUG
            return PlanetVisibility.Scanned;
#else
            if (archive == null || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return PlanetVisibility.Hidden;
            }

            if (knowledge.HasScannedPlanet(planetId))
            {
                return PlanetVisibility.Scanned;
            }

            if (!systemId.IsEmpty && knowledge.HasExploredNode(systemId))
            {
                return PlanetVisibility.SystemExplored;
            }

            return PlanetVisibility.Hidden;
#endif
        }

        /// <summary>
        /// Whether a route should appear on the star map for the observer.
        /// </summary>
        public bool IsRouteVisibleOnMap(
            RunArchive archive,
            ObserverId observerId,
            ContentId routeKey)
        {
            if (routeKey.IsEmpty)
            {
                return false;
            }

#if STARWATCH_DEBUG
            return true;
#else
            return archive != null
                && archive.TryGetMapKnowledge(observerId, out var knowledge)
                && knowledge.HasKnownRoute(routeKey);
#endif
        }
    }
}
