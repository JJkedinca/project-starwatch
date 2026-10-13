using System.Collections.Generic;
using EndlessHeros.Core.Data;

namespace EndlessHeros.Core.Content
{
    /// <summary>
    /// Lookup facade from stable content IDs to authored ScriptableObject definitions.
    /// </summary>
    public sealed class ContentCatalog
    {
        private readonly Dictionary<string, ScenarioDefinition> _scenarios = new();

        private readonly Dictionary<string, PlanetDefinition> _planets = new();

        public ContentCatalog()
        {
            RegisterScenario(VerticalSliceScenarioFactory.Create());
        }

        /// <summary>
        /// Registers or replaces a scenario definition by its scenario id.
        /// </summary>
        public void RegisterScenario(ScenarioDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ScenarioId))
            {
                return;
            }

            _scenarios[definition.ScenarioId] = definition;

            if (definition.Planets == null)
            {
                return;
            }

            for (var i = 0; i < definition.Planets.Count; i++)
            {
                var planet = definition.Planets[i];
                if (planet == null || planet.Id.IsEmpty)
                {
                    continue;
                }

                _planets[planet.Id.Value] = planet;
            }
        }

        /// <summary>
        /// Resolves a scenario definition by scenario id.
        /// </summary>
        public bool TryGetScenario(string scenarioId, out ScenarioDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                definition = null;
                return false;
            }

            return _scenarios.TryGetValue(scenarioId, out definition);
        }

        /// <summary>
        /// Resolves a planet definition by content id value.
        /// </summary>
        public bool TryGetPlanet(string planetId, out PlanetDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(planetId))
            {
                definition = null;
                return false;
            }

            return _planets.TryGetValue(planetId, out definition);
        }
    }
}
