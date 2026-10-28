using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Generation;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectStarwatch.Core.Content
{
    /// <summary>
    /// Lookup from stable content IDs to authored definitions and runtime-registered scenarios.
    /// Generated galaxies register via <see cref="RegisterScenario"/> on CreateRun.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectDatabaseSO", menuName = "Starwatch/Object Database")]
    public sealed class ObjectDatabaseSO : ScriptableObject
    {
        [field: SerializeField]
        public SettlementCatalogSO SettlementCatalog { get; private set; }

        [field: SerializeField]
        public ResearchCatalogSO ResearchCatalog { get; private set; }

        [field: SerializeField]
        public ShipCatalogSO ShipCatalog { get; private set; }

        [field: SerializeField]
        public GalaxyCatalogSO GalaxyCatalog { get; private set; }

        [field: SerializeField]
        public PlanetDatabaseSO PlanetDatabase { get; private set; }

        [field: SerializeField]
        public StarSystemDatabaseSO StarSystemDatabase { get; private set; }

        private Dictionary<string, ScenarioDefinition> _scenarios = new();
        public IReadOnlyDictionary<string, ScenarioDefinition> Scenarios => _scenarios;

        private Dictionary<string, PlanetDefinition> _planets = new();
        private Dictionary<string, string> _systemDisplayNames = new();

        private bool _indexed;

        private void OnEnable()
        {
            // Domain reload keeps this asset and drops runtime scenarios. The next lookup must rebuild.
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
            Assert.IsNotNull(
                SettlementCatalog,
                "ObjectDatabaseSO requires an assigned SettlementCatalogSO.");
            Assert.IsNotNull(
                ResearchCatalog,
                "ObjectDatabaseSO requires an assigned ResearchCatalogSO.");
            Assert.IsNotNull(
                ShipCatalog,
                "ObjectDatabaseSO requires an assigned ShipCatalogSO.");
            Assert.IsNotNull(
                PlanetDatabase,
                "ObjectDatabaseSO requires an assigned PlanetDatabaseSO.");
            Assert.IsNotNull(
                StarSystemDatabase,
                "ObjectDatabaseSO requires an assigned StarSystemDatabaseSO.");
            GalaxyCatalog.ValidateRequired();
            PlanetDatabase.ValidateRequired();
            StarSystemDatabase.ValidateRequired();
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
        /// Resolves a generated or registered star-system display name by content id value.
        /// </summary>
        public bool TryGetStarSystemDisplayName(string systemId, out string displayName)
        {
            EnsureIndexed();
            if (string.IsNullOrWhiteSpace(systemId))
            {
                displayName = null;
                return false;
            }

            return _systemDisplayNames.TryGetValue(systemId, out displayName)
                && !string.IsNullOrEmpty(displayName);
        }

        /// <summary>
        /// Points a runtime instance at catalogs. Used by EditMode tests.
        /// </summary>
        internal void AssignDatabases(
            SettlementCatalogSO settlementCatalog = null,
            ResearchCatalogSO researchCatalog = null,
            ShipCatalogSO shipCatalog = null,
            GalaxyCatalogSO galaxyCatalog = null,
            PlanetDatabaseSO planetDatabase = null,
            StarSystemDatabaseSO starSystemDatabase = null)
        {
            SettlementCatalog = settlementCatalog;
            ResearchCatalog = researchCatalog;
            ShipCatalog = shipCatalog;
            GalaxyCatalog = galaxyCatalog;
            PlanetDatabase = planetDatabase;
            StarSystemDatabase = starSystemDatabase;
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
        /// Scenario ids available to start a run.
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
        /// Registers or replaces a scenario definition by its scenario id.
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
            _systemDisplayNames ??= new Dictionary<string, string>();
            ClearIndex();
            _indexed = true;
        }

        private void EnsureIndexed()
        {
            _scenarios ??= new Dictionary<string, ScenarioDefinition>();
            _planets ??= new Dictionary<string, PlanetDefinition>();
            _systemDisplayNames ??= new Dictionary<string, string>();
            if (_indexed && !IndexNeedsRebuild())
            {
                return;
            }

            ClearIndex();
            _indexed = true;
        }

        private bool IndexNeedsRebuild()
        {
            if (_scenarios == null)
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
            _systemDisplayNames.Clear();
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
            IndexScenarioSystems(definition);
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

            if (scenario.Nodes == null)
            {
                return;
            }

            for (var i = 0; i < scenario.Nodes.Count; i++)
            {
                var node = scenario.Nodes[i];
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                _systemDisplayNames.Remove(node.Id.Value);
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

        private void IndexScenarioSystems(ScenarioDefinition scenario)
        {
            if (scenario.Nodes == null)
            {
                return;
            }

            for (var i = 0; i < scenario.Nodes.Count; i++)
            {
                var node = scenario.Nodes[i];
                if (node == null || node.Id.IsEmpty || string.IsNullOrEmpty(node.DisplayName))
                {
                    continue;
                }

                _systemDisplayNames[node.Id.Value] = node.DisplayName;
            }
        }
    }
}
