using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored adventure module: map nodes, routes, planets, and starting empire.
    /// </summary>
    public sealed class ScenarioDefinition : ScriptableObject
    {
        public string ScenarioId { get; private set; }

        public StartingEmpireDefinition StartingEmpire { get; private set; }

        public IReadOnlyList<MapNodeDefinition> Nodes { get; private set; }

        public IReadOnlyList<MapRouteDefinition> Routes { get; private set; }

        public IReadOnlyList<PlanetDefinition> Planets { get; private set; }

        /// <summary>
        /// Configures this instance from factory or editor tooling.
        /// </summary>
        public void Configure(
            string scenarioId,
            StartingEmpireDefinition startingEmpire,
            IReadOnlyList<MapNodeDefinition> nodes,
            IReadOnlyList<MapRouteDefinition> routes,
            IReadOnlyList<PlanetDefinition> planets)
        {
            ScenarioId = scenarioId ?? string.Empty;
            StartingEmpire = startingEmpire ?? StartingEmpireDefinition.CreateDefault();
            Nodes = nodes ?? System.Array.Empty<MapNodeDefinition>();
            Routes = routes ?? System.Array.Empty<MapRouteDefinition>();
            Planets = planets ?? System.Array.Empty<PlanetDefinition>();
        }
    }
}
