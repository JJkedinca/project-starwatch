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
            if (archive == null
                || nodeId.IsEmpty
                || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return MapNodeVisibility.Hidden;
            }

            if (timeline != null && timeline.HasFleetAt(nodeId))
            {
                return MapNodeVisibility.Present;
            }

            return knowledge.HasExploredNode(nodeId)
                ? MapNodeVisibility.Explored
                : MapNodeVisibility.Hidden;
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
            if (archive == null
                || planetId.IsEmpty
                || !archive.TryGetMapKnowledge(observerId, out var knowledge))
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
        }
    }
}
