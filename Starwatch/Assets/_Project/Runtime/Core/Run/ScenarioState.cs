using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Stable run-level content generated once at run start and preserved across rewinds.
    /// Owns its collections; timeline and archive must never alias these lists.
    /// </summary>
    public sealed class ScenarioState
    {
        public string ScenarioId { get; }

        public StartingEmpireDefinition StartingEmpire { get; }

        /// <summary>
        /// Scenario-owned system ids. Phase 1 will populate the full authored graph.
        /// </summary>
        public IReadOnlyList<ContentId> SystemIds => _systemIds;

        private readonly List<ContentId> _systemIds = new List<ContentId>();

        public ScenarioState(string scenarioId, StartingEmpireDefinition startingEmpire)
        {
            ScenarioId = scenarioId ?? string.Empty;
            StartingEmpire = startingEmpire ?? StartingEmpireDefinition.CreateDefault();

            TryAddSystemId(StartingEmpire.HomeSystemId);
        }

        /// <summary>
        /// Adds a system id when non-empty and not already present.
        /// </summary>
        public bool TryAddSystemId(ContentId systemId)
        {
            if (systemId.IsEmpty || _systemIds.Contains(systemId))
            {
                return false;
            }

            _systemIds.Add(systemId);
            return true;
        }
    }
}
