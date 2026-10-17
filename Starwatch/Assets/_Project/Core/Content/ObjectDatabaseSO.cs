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

        [field: SerializeField]
        public SettlementCatalogSO SettlementCatalog { get; private set; }

        private Dictionary<string, ScenarioDefinition> _scenarios = new();
        public IReadOnlyDictionary<string, ScenarioDefinition> Scenarios => _scenarios;

        private Dictionary<string, PlanetDefinition> _planets = new();

        private bool _indexed;

        private void OnEnable()
        {
            // Domain reload keeps this asset and drops the runtime scenario. The next lookup must rebuild.
            _indexed = false;
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

        /// <summary>
        /// Points a runtime instance at the authored databases. Used by EditMode tests.
        /// </summary>
        internal void AssignDatabases(
            StarSystemDatabaseSO starSystemDatabase,
            PlanetDatabaseSO planetDatabase,
            SettlementCatalogSO settlementCatalog = null)
        {
            _starSystemDatabase = starSystemDatabase;
            _planetDatabase = planetDatabase;
            SettlementCatalog = settlementCatalog;
            _indexed = false;
        }

        /// <summary>
        /// Registers or replaces a scenario definition by its scenario id. Used by EditMode tests.
        /// </summary>
        internal void RegisterScenario(ScenarioDefinition definition)
        {
            EnsureIndexed();
            AddScenario(definition);
        }

        /// <summary>
        /// Clears the runtime scenario index and leaves it marked built.
        /// EditMode tests use this to simulate a domain reload. The next lookup must rebuild.
        /// </summary>
        internal void ResetBuiltIndex()
        {
            _scenarios ??= new Dictionary<string, ScenarioDefinition>();
            _planets ??= new Dictionary<string, PlanetDefinition>();
            ClearIndex();
            _indexed = true;
        }

        private void EnsureIndexed()
        {
            _scenarios ??= new Dictionary<string, ScenarioDefinition>();
            _planets ??= new Dictionary<string, PlanetDefinition>();
            if (_indexed && !IndexNeedsRebuild())
            {
                return;
            }

            // Play-mode exit and script recompile destroy the runtime scenario while this asset stays.
            ClearIndex();
            if (_starSystemDatabase == null || _planetDatabase == null)
            {
                _indexed = false;
                Debug.LogError("ObjectDatabaseSO requires an assigned StarSystemDatabaseSO and PlanetDatabaseSO.");
                return;
            }

            AddScenario(VerticalSliceScenarioFactory.Create(_starSystemDatabase, _planetDatabase));
            _indexed = _scenarios.Count > 0;
        }

        private bool IndexNeedsRebuild()
        {
            if (_scenarios == null || _scenarios.Count == 0)
            {
                return true;
            }

            foreach (var pair in _scenarios)
            {
                if (pair.Value == null || string.IsNullOrWhiteSpace(pair.Value.ScenarioId))
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearIndex()
        {
            foreach (var pair in _scenarios)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(pair.Value);
                }
                else
                {
                    DestroyImmediate(pair.Value);
                }
            }

            _scenarios.Clear();
            _planets.Clear();
        }

        private void AddScenario(ScenarioDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ScenarioId))
            {
                return;
            }

            definition.hideFlags = HideFlags.HideAndDontSave;
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
