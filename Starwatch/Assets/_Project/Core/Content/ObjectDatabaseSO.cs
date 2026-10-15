using System.Collections.Generic;
using EndlessHeros.Core.Data;
using UnityEngine;

namespace EndlessHeros.Core.Content
{
    /// <summary>
    /// Lookup from stable content IDs to authored definitions.
    /// Assign source databases on the asset. Indexes are built on first use.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectDatabaseSO", menuName = "Endless Heros/Object Database")]
    public sealed class ObjectDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private StarSystemDatabaseSO _starSystemDatabase;

        [SerializeField]
        private PlanetDatabaseSO _planetDatabase;

        private readonly Dictionary<string, ScenarioDefinition> _scenarios = new();

        private readonly Dictionary<string, PlanetDefinition> _planets = new();

        private bool _indexed;

        /// <summary>
        /// Points a runtime instance at the authored databases. Used by EditMode tests.
        /// </summary>
        internal void AssignDatabases(StarSystemDatabaseSO starSystemDatabase, PlanetDatabaseSO planetDatabase)
        {
            _starSystemDatabase = starSystemDatabase;
            _planetDatabase = planetDatabase;
        }

        /// <summary>
        /// Registers or replaces a scenario definition by its scenario id.
        /// </summary>
        public void RegisterScenario(ScenarioDefinition definition)
        {
            EnsureIndexed();
            AddScenario(definition);
        }

        /// <summary>
        /// Resolves a scenario definition by scenario id.
        /// </summary>
        public bool TryGetScenario(string scenarioId, out ScenarioDefinition definition)
        {
            EnsureIndexed();
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                definition = null;
                return false;
            }

            return _scenarios.TryGetValue(scenarioId, out definition) && definition != null;
        }

        /// <summary>
        /// Resolves a planet definition by content id value.
        /// </summary>
        public bool TryGetPlanet(string planetId, out PlanetDefinition definition)
        {
            EnsureIndexed();
            if (string.IsNullOrWhiteSpace(planetId))
            {
                definition = null;
                return false;
            }

            return _planets.TryGetValue(planetId, out definition);
        }

        private void EnsureIndexed()
        {
            if (_indexed && !CachedScenarioWasDestroyed())
            {
                return;
            }

            // Exit play mode destroys the runtime ScenarioDefinition. This asset outlives
            // that object when the editor keeps its memory, so rebuild a destroyed entry.
            _scenarios.Clear();
            _planets.Clear();
            _indexed = true;
            if (_starSystemDatabase == null || _planetDatabase == null)
            {
                Debug.LogError("ObjectDatabaseSO requires an assigned StarSystemDatabaseSO and PlanetDatabaseSO.");
                return;
            }

            AddScenario(VerticalSliceScenarioFactory.Create(_starSystemDatabase, _planetDatabase));
        }

        private bool CachedScenarioWasDestroyed()
        {
            foreach (var pair in _scenarios)
            {
                if (pair.Value == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddScenario(ScenarioDefinition definition)
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
    }
}
