using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Archive
{
    /// <summary>
    /// Persistent within-run knowledge and unlocks that survive timeline resets.
    /// Owns its collections; must not alias universe or timeline collections.
    /// </summary>
    public sealed class RunArchive
    {
        /// <summary>
        /// Player map knowledge for Phase 1. Multi-observer slots arrive with factions.
        /// </summary>
        public MapKnowledge PlayerMapKnowledge { get; }

        /// <summary>
        /// Completed within-run research. Survives a timeline reset.
        /// </summary>
        public ResearchKnowledge Research { get; }

        public RunArchive()
        {
            PlayerMapKnowledge = new MapKnowledge(ObserverId.Player);
            Research = new ResearchKnowledge();
        }

        /// <summary>
        /// Convenience for player explore writes used by Phase 0 stubs and tests.
        /// Returns true only when the node was newly added.
        /// </summary>
        public bool TryAddExploredNode(ContentId nodeId)
        {
            return PlayerMapKnowledge.TryAddExploredNode(nodeId);
        }

        /// <summary>
        /// Returns map knowledge for the observer. Phase 1 only has the player slot.
        /// </summary>
        public bool TryGetMapKnowledge(ObserverId observerId, out MapKnowledge knowledge)
        {
            if (observerId == ObserverId.Player || observerId.IsEmpty)
            {
                knowledge = PlayerMapKnowledge;
                return true;
            }

            knowledge = null;
            return false;
        }
    }
}
