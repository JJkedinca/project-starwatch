using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Universe
{
    /// <summary>
    /// Fixed run-level universe: authored map graph and starting empire.
    /// Survives rewind; must not be aliased by timeline or archive collections.
    /// </summary>
    public sealed class UniverseState
    {
        public string ScenarioId { get; }

        public StartingEmpireDefinition StartingEmpire { get; }

        public MapGraph MapGraph { get; }

        /// <summary>
        /// Universe-owned system (and node) ids present on the map graph.
        /// </summary>
        public IReadOnlyList<ContentId> NodeIds => _nodeIds;

        private readonly List<ContentId> _nodeIds = new();

        public UniverseState(string scenarioId, StartingEmpireDefinition startingEmpire, MapGraph mapGraph)
        {
            ScenarioId = scenarioId ?? string.Empty;
            StartingEmpire = startingEmpire ?? StartingEmpireDefinition.CreateDefault();
            MapGraph = mapGraph ?? MapGraph.Empty;

            foreach (var node in MapGraph.Nodes)
            {
                TryAddNodeId(node.Id);
            }

            TryAddNodeId(StartingEmpire.HomeSystemId);
        }

        /// <summary>
        /// Adds a node id when non-empty and not already present.
        /// </summary>
        public bool TryAddNodeId(ContentId nodeId)
        {
            if (nodeId.IsEmpty || _nodeIds.Contains(nodeId))
            {
                return false;
            }

            _nodeIds.Add(nodeId);
            return true;
        }
    }
}
