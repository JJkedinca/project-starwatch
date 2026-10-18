using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Archive
{
    /// <summary>
    /// Completed research and the projects currently available in the run. Owned by <see cref="RunArchive"/>.
    /// </summary>
    public sealed class ResearchKnowledge : IResearchKnowledgeQuery
    {
        private readonly HashSet<ContentId> _completedIds = new();
        private readonly List<ContentId> _projects = new();
        private readonly HashSet<ContentId> _projectIds = new();
        private bool _projectsSeeded;

        public IReadOnlyCollection<ContentId> CompletedIds => _completedIds;

        public IReadOnlyList<ContentId> Projects => _projects;

        internal bool ProjectsSeeded => _projectsSeeded;

        public bool IsAvailable(ContentId researchId)
        {
            return !researchId.IsEmpty && _projectIds.Contains(researchId);
        }

        /// <summary>
        /// Adds a research to the run's available projects. Returns true only when newly added.
        /// </summary>
        public bool TryAddProject(ContentId researchId)
        {
            if (researchId.IsEmpty || !_projectIds.Add(researchId))
            {
                return false;
            }

            _projects.Add(researchId);
            return true;
        }

        internal void MarkProjectsSeeded()
        {
            _projectsSeeded = true;
        }

        public bool Contains(ContentId researchId)
        {
            return !researchId.IsEmpty && _completedIds.Contains(researchId);
        }

        /// <summary>
        /// Adds a completed project when non-empty. Returns true only when newly added.
        /// </summary>
        public bool TryAdd(ContentId researchId)
        {
            return !researchId.IsEmpty && _completedIds.Add(researchId);
        }
    }
}
