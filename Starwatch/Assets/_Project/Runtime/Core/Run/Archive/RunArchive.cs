using System.Collections.Generic;
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
        /// Completed within-run research unlocks. Phase 3 fills this.
        /// </summary>
        public IReadOnlyCollection<ContentId> CompletedResearchIds => _completedResearchIds;

        private readonly HashSet<ContentId> _completedResearchIds = new();

        public RunArchive()
        {
            PlayerMapKnowledge = new MapKnowledge(ObserverId.Player);
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
        /// Adds completed research when non-empty. Returns true only when newly added.
        /// </summary>
        public bool TryAddCompletedResearch(ContentId researchId)
        {
            return !researchId.IsEmpty && _completedResearchIds.Add(researchId);
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
