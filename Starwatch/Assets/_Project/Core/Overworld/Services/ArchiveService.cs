using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Archive;

namespace EndlessHeros.Overworld.Services
{
    /// <summary>
    /// Thin façade for map-knowledge writes on the run archive.
    /// </summary>
    public sealed class ArchiveService
    {
        /// <summary>
        /// Records an explored node for the observer when knowledge exists.
        /// </summary>
        public bool TryRecordExploredNode(RunArchive archive, ObserverId observerId, ContentId nodeId)
        {
            if (archive == null || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return false;
            }

            return knowledge.TryAddExploredNode(nodeId);
        }

        /// <summary>
        /// Records a known route for the observer when knowledge exists.
        /// </summary>
        public bool TryRecordKnownRoute(RunArchive archive, ObserverId observerId, ContentId routeKey)
        {
            if (archive == null || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return false;
            }

            return knowledge.TryAddKnownRoute(routeKey);
        }

        /// <summary>
        /// Records a scanned planet for the observer when knowledge exists.
        /// </summary>
        public bool TryRecordScannedPlanet(RunArchive archive, ObserverId observerId, ContentId planetId)
        {
            if (archive == null || !archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return false;
            }

            return knowledge.TryAddScannedPlanet(planetId);
        }
    }
}
