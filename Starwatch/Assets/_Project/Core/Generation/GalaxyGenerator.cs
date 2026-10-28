using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Seed-driven galaxy scenario builder: place nodes, connect routes, emit planets and starting empire.
    /// </summary>
    public sealed class GalaxyGenerator
    {
        public const string ScenarioId = "generated-galaxy";

        private const int kPlanetsPerSystem = 2;
        private const int kHomeExtraPlanet = 1;
        /// <summary>
        /// Matches authored planet budgets so the starter colony (4× footprint-2 producers) fits.
        /// </summary>
        private const int kPlanetFootprint = 10;
        private const int kStarterShipCount = 3;
        private const int kStartingIndustry = 80;
        private const int kStartingEnergy = 40;

        private readonly GalaxyCatalogSO _catalog;

        public GalaxyGenerator(GalaxyCatalogSO catalog)
        {
            _catalog = catalog;
        }

        /// <summary>
        /// Builds a <see cref="ScenarioDefinition"/> for <see cref="ScenarioId"/> from the run seed
        /// and the selected size/shape ids in the galaxy catalog.
        /// </summary>
        public bool TryCreateScenario(
            RunSeed seed,
            GalaxySizeId sizeId,
            GalaxyShapeId shapeId,
            SettlementCatalogSO settlementCatalog,
            out ScenarioDefinition definition)
        {
            definition = null;
            if (_catalog == null)
            {
                Debug.LogError("GalaxyGenerator.TryCreateScenario requires a galaxy catalog.");
                return false;
            }

            if (!_catalog.TryGetSize(sizeId, out var sizeOption))
            {
                Debug.LogError($"GalaxyGenerator.TryCreateScenario could not find galaxy size '{sizeId}'.");
                return false;
            }

            if (!_catalog.TryGetShape(shapeId, out var shapeOption))
            {
                Debug.LogError($"GalaxyGenerator.TryCreateScenario could not find galaxy shape '{shapeId}'.");
                return false;
            }

            var settings = sizeOption.Settings;
            var shape = shapeOption.Shape;
            if (!TryPlaceLayout(seed, settings, shape, out var layout))
            {
                Debug.LogError(
                    $"GalaxyGenerator.TryCreateScenario failed placement after {settings.MaxPlacementAttempts} attempts (seed {seed.Value}).");
                return false;
            }

            var nodeCount = layout.Nodes.Count;
            var homeIndex = layout.HomeIndex;
            if (homeIndex < 0 || homeIndex >= nodeCount)
            {
                Debug.LogError("GalaxyGenerator.TryCreateScenario got an out-of-range home index.");
                return false;
            }

            var nodeIds = new ContentId[nodeCount];
            var mapPositions = new Vector2[nodeCount];
            var scale = Mathf.Max(0.001f, settings.MapScale);
            for (var i = 0; i < nodeCount; i++)
            {
                nodeIds[i] = NodeId(i);
                mapPositions[i] = layout.Nodes[i].Position * scale;
            }

            if (!GalaxyRouteConnector.TryConnect(
                    nodeIds,
                    mapPositions,
                    layout.Nodes,
                    shape,
                    settings.TargetMedianTravelSeconds,
                    out var routes))
            {
                return false;
            }

            BuildPlanetsAndNodes(
                nodeIds,
                mapPositions,
                homeIndex,
                out var nodes,
                out var planets,
                out var homePlanetId);

            var startingEmpire = new StartingEmpireDefinition(
                nodeIds[homeIndex],
                ContentId.From("starter-screen"),
                kStarterShipCount,
                new StartingResources(industry: kStartingIndustry, energy: kStartingEnergy),
                HomeColony(homePlanetId, settlementCatalog));

            definition = ScriptableObject.CreateInstance<ScenarioDefinition>();
            definition.Configure(ScenarioId, startingEmpire, nodes, routes, planets);
            return true;
        }

        private static bool TryPlaceLayout(
            RunSeed seed,
            GenerationSettingsSO settings,
            GalaxyShapeSO shape,
            out GeneratedGalaxyLayout layout)
        {
            layout = null;
            var maxAttempts = Mathf.Max(1, settings.MaxPlacementAttempts);
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var random = new System.Random(unchecked(seed.Value + attempt * 9973));
                if (shape.TryPlaceNodes(settings, random, out layout))
                {
                    return true;
                }
            }

            return false;
        }

        private static void BuildPlanetsAndNodes(
            IReadOnlyList<ContentId> nodeIds,
            IReadOnlyList<Vector2> mapPositions,
            int homeIndex,
            out List<MapNodeDefinition> nodes,
            out List<PlanetDefinition> planets,
            out ContentId homePlanetId)
        {
            nodes = new List<MapNodeDefinition>(nodeIds.Count);
            planets = new List<PlanetDefinition>(nodeIds.Count * (kPlanetsPerSystem + 1));
            homePlanetId = default;
            var neutral = new PlanetModifiers(1f, 1f, 1f, 1f);

            for (var i = 0; i < nodeIds.Count; i++)
            {
                var systemId = nodeIds[i];
                var planetCount = kPlanetsPerSystem + (i == homeIndex ? kHomeExtraPlanet : 0);
                var planetIds = new ContentId[planetCount];
                for (var planetIndex = 0; planetIndex < planetCount; planetIndex++)
                {
                    var planetId = PlanetId(i, planetIndex);
                    planetIds[planetIndex] = planetId;
                    var displayName = i == homeIndex && planetIndex == 0
                        ? "Homeworld"
                        : $"Planet {planetIndex + 1}";
                    planets.Add(new PlanetDefinition(
                        planetId,
                        systemId,
                        displayName,
                        summary: string.Empty,
                        kPlanetFootprint,
                        deposits: null,
                        neutral));
                }

                if (i == homeIndex)
                {
                    homePlanetId = planetIds[0];
                }

                nodes.Add(new MapNodeDefinition(systemId, MapNodeKind.System, mapPositions[i], planetIds));
            }
        }

        private static List<StartingSettlement> HomeColony(
            ContentId homePlanetId,
            SettlementCatalogSO catalog)
        {
            var buildings = new List<StartingBuilding>();
            var catalogBuildings = catalog != null ? catalog.Buildings : null;
            if (catalogBuildings == null)
            {
                Debug.LogWarning(
                    "GalaxyGenerator has no settlement catalog, so the home colony has no buildings.");
            }
            else
            {
                for (var i = 0; i < catalogBuildings.Count; i++)
                {
                    var building = catalogBuildings[i];
                    if (building == null || building.Id.IsEmpty || !Produces(building))
                    {
                        continue;
                    }

                    buildings.Add(new StartingBuilding(building.Id, 1));
                }
            }

            return new List<StartingSettlement>
            {
                new(homePlanetId, isColony: true, buildings),
            };
        }

        private static bool Produces(BuildingDefinitionSO building)
        {
            var output = building.Output;
            for (var i = 0; i < output.Count; i++)
            {
                if (output[i].Amount > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private static ContentId NodeId(int index)
        {
            return ContentId.From($"gen-node-{index}");
        }

        private static ContentId PlanetId(int nodeIndex, int planetIndex)
        {
            return ContentId.From($"gen-node-{nodeIndex}-{planetIndex}");
        }
    }
}
