using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Generation;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectStarwatch.Core.Content
{
    /// <summary>
    /// Lookup from stable content IDs to authored definitions.
    /// Assign source databases on the asset. Indexes are built on first use.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectDatabaseSO", menuName = "Starwatch/Object Database")]
    public sealed class ObjectDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private StarSystemDatabaseSO _starSystemDatabase;

        [SerializeField]
        private PlanetDatabaseSO _planetDatabase;

        [field: SerializeField]
        public SettlementCatalogSO SettlementCatalog { get; private set; }

        [field: SerializeField]
        public ResearchCatalogSO ResearchCatalog { get; private set; }

        [field: SerializeField]
        public ShipCatalogSO ShipCatalog { get; private set; }

        [field: SerializeField]
        public GalaxyCatalogSO GalaxyCatalog { get; private set; }

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
        /// Asserts required catalogs are wired. Called at project bootstrap — generation cannot run without them.
        /// </summary>
        public void ValidateRequired()
        {
            Assert.IsNotNull(
                GalaxyCatalog,
                "ObjectDatabaseSO requires an assigned GalaxyCatalogSO.");
            GalaxyCatalog.ValidateRequired();
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
        /// Resolves an authored star system or non-system node by content id value.
        /// </summary>
        public bool TryGetStarSystem(string systemId, out StarSystemDefinition definition)
        {
            definition = null;
            var systems = _starSystemDatabase != null ? _starSystemDatabase.StarSystems : null;
            if (string.IsNullOrWhiteSpace(systemId) || systems == null)
            {
                return false;
            }

            for (var i = 0; i < systems.Count; i++)
            {
                var system = systems[i];
                if (system != null && system.Id.Value == systemId)
                {
                    definition = system;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Points a runtime instance at the authored databases. Used by EditMode tests.
        /// </summary>
        internal void AssignDatabases(
            StarSystemDatabaseSO starSystemDatabase,
            PlanetDatabaseSO planetDatabase,
            SettlementCatalogSO settlementCatalog = null,
            ResearchCatalogSO researchCatalog = null,
            ShipCatalogSO shipCatalog = null,
            GalaxyCatalogSO galaxyCatalog = null)
        {
            _starSystemDatabase = starSystemDatabase;
            _planetDatabase = planetDatabase;
            SettlementCatalog = settlementCatalog;
            ResearchCatalog = researchCatalog;
            ShipCatalog = shipCatalog;
            GalaxyCatalog = galaxyCatalog;
            _indexed = false;
        }

        /// <summary>
        /// Points a runtime instance at the galaxy catalog. Used by EditMode tests.
        /// </summary>
        internal void AssignGalaxyCatalog(GalaxyCatalogSO galaxyCatalog)
        {
            GalaxyCatalog = galaxyCatalog;
        }

        /// <summary>
        /// Scenario ids available to start a run: authored index plus generated-galaxy.
        /// </summary>
        public List<string> ListScenarioIds()
        {
            EnsureIndexed();
            List<string> ids = new();
            foreach (var pair in _scenarios)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                {
                    ids.Add(pair.Key);
                }
            }

            if (!ids.Contains(GalaxyGenerator.ScenarioId))
            {
                ids.Add(GalaxyGenerator.ScenarioId);
            }

            ids.Sort(System.StringComparer.Ordinal);
            return ids;
        }

        /// <summary>
        /// Points a runtime instance at a research catalog. Used by EditMode tests.
        /// </summary>
        internal void AssignResearchCatalog(ResearchCatalogSO researchCatalog)
        {
            ResearchCatalog = researchCatalog;
        }

        /// <summary>
        /// Points a runtime instance at a ship catalog. Used by EditMode tests.
        /// </summary>
        internal void AssignShipCatalog(ShipCatalogSO shipCatalog)
        {
            ShipCatalog = shipCatalog;
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

            AddScenario(VerticalSliceScenarioFactory.Create(
                _starSystemDatabase,
                _planetDatabase,
                SettlementCatalog));
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

            if (_scenarios.TryGetValue(definition.ScenarioId, out var previous)
                && previous != null
                && previous != definition)
            {
                RemoveScenarioPlanets(previous);
                if (Application.isPlaying)
                {
                    Destroy(previous);
                }
                else
                {
                    DestroyImmediate(previous);
                }
            }

            definition.hideFlags = HideFlags.HideAndDontSave;
            _scenarios[definition.ScenarioId] = definition;
            IndexScenarioPlanets(definition);
        }

        private void RemoveScenarioPlanets(ScenarioDefinition scenario)
        {
            if (scenario?.Planets == null)
            {
                return;
            }

            for (var i = 0; i < scenario.Planets.Count; i++)
            {
                var planet = scenario.Planets[i];
                if (planet == null || planet.Id.IsEmpty)
                {
                    continue;
                }

                _planets.Remove(planet.Id.Value);
            }
        }

        private void IndexScenarioPlanets(ScenarioDefinition scenario)
        {
            if (scenario.Planets == null)
            {
                return;
            }

            for (var i = 0; i < scenario.Planets.Count; i++)
            {
                var planet = scenario.Planets[i];
                if (planet == null || planet.Id.IsEmpty)
                {
                    continue;
                }

                _planets[planet.Id.Value] = planet;
            }
        }
    }
}
