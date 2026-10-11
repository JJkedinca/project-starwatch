using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Persistent within-run knowledge and unlocks that survive timeline resets.
    /// Owns its collections; must not alias scenario or timeline lists.
    /// </summary>
    public sealed class RunArchive
    {
        /// <summary>
        /// Map nodes recorded as explored. Phase 1 fills this through exploration.
        /// </summary>
        public IReadOnlyCollection<ContentId> ExploredNodeIds => _exploredNodeIds;

        /// <summary>
        /// Completed within-run research unlocks. Phase 3 fills this.
        /// </summary>
        public IReadOnlyCollection<ContentId> CompletedResearchIds => _completedResearchIds;

        private readonly HashSet<ContentId> _exploredNodeIds = new HashSet<ContentId>();
        private readonly HashSet<ContentId> _completedResearchIds = new HashSet<ContentId>();

        /// <summary>
        /// Records an explored node when non-empty. Idempotent for duplicates.
        /// </summary>
        public bool TryAddExploredNode(ContentId nodeId)
        {
            if (nodeId.IsEmpty)
            {
                return false;
            }

            _exploredNodeIds.Add(nodeId);
            return true;
        }

        /// <summary>
        /// Records completed research when non-empty. Idempotent for duplicates.
        /// </summary>
        public bool TryAddCompletedResearch(ContentId researchId)
        {
            if (researchId.IsEmpty)
            {
                return false;
            }

            _completedResearchIds.Add(researchId);
            return true;
        }
    }
}
