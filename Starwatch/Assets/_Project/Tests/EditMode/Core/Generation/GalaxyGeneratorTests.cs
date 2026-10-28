using System.Collections.Generic;
using NUnit.Framework;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Generation;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.Generation
{
    /// <summary>
    /// EditMode coverage for seed-driven galaxy scenario generation.
    /// </summary>
    public sealed class GalaxyGeneratorTests
    {
        [Test]
        public void TryCreateScenario_SameSeed_ProducesSameScenario()
        {
            var poolsA = CreatePools(out var catalogA, out var settingsA, out var shapeA);
            var poolsB = CreatePools(out var catalogB, out var settingsB, out var shapeB);
            var generatorA = new GalaxyGenerator(catalogA, poolsA.Planets, poolsA.Systems);
            var generatorB = new GalaxyGenerator(catalogB, poolsB.Planets, poolsB.Systems);
            var seed = RunSeed.FromValue(12345);

            Assert.That(
                generatorA.TryCreateScenario(
                    seed,
                    GalaxyCatalogSO.DefaultSizeId,
                    GalaxyCatalogSO.DefaultShapeId,
                    settlementCatalog: null,
                    out var first),
                Is.True);
            Assert.That(
                generatorB.TryCreateScenario(
                    seed,
                    GalaxyCatalogSO.DefaultSizeId,
                    GalaxyCatalogSO.DefaultShapeId,
                    settlementCatalog: null,
                    out var second),
                Is.True);

            Assert.That(first.ScenarioId, Is.EqualTo(GalaxyGenerator.ScenarioId));
            Assert.That(first.Nodes.Count, Is.EqualTo(second.Nodes.Count));
            Assert.That(first.Routes.Count, Is.EqualTo(second.Routes.Count));
            Assert.That(first.Planets.Count, Is.EqualTo(second.Planets.Count));
            Assert.That(first.StartingEmpire.HomeSystemId, Is.EqualTo(second.StartingEmpire.HomeSystemId));

            for (var i = 0; i < first.Nodes.Count; i++)
            {
                Assert.That(first.Nodes[i].Id, Is.EqualTo(second.Nodes[i].Id));
                Assert.That(first.Nodes[i].MapPosition, Is.EqualTo(second.Nodes[i].MapPosition));
                Assert.That(first.Nodes[i].DisplayName, Is.EqualTo(second.Nodes[i].DisplayName));
            }

            for (var i = 0; i < first.Planets.Count; i++)
            {
                Assert.That(first.Planets[i].DisplayName, Is.EqualTo(second.Planets[i].DisplayName));
                Assert.That(first.Planets[i].FootprintBudget, Is.EqualTo(second.Planets[i].FootprintBudget));
            }

            for (var i = 0; i < first.Routes.Count; i++)
            {
                Assert.That(first.Routes[i].NodeAId, Is.EqualTo(second.Routes[i].NodeAId));
                Assert.That(first.Routes[i].NodeBId, Is.EqualTo(second.Routes[i].NodeBId));
                Assert.That(first.Routes[i].TravelDurationSeconds, Is.EqualTo(second.Routes[i].TravelDurationSeconds));
            }

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            DestroyPools(poolsA, catalogA, settingsA, shapeA);
            DestroyPools(poolsB, catalogB, settingsB, shapeB);
        }

        [Test]
        public void TryCreateScenario_UniverseLoaderAcceptsGeneratedScenario()
        {
            var pools = CreatePools(out var catalog, out var settings, out var shape);
            var generator = new GalaxyGenerator(catalog, pools.Planets, pools.Systems);
            Assert.That(
                generator.TryCreateScenario(
                    RunSeed.FromValue(77),
                    GalaxyCatalogSO.DefaultSizeId,
                    GalaxyCatalogSO.DefaultShapeId,
                    settlementCatalog: null,
                    out var scenario),
                Is.True);

            var loader = new UniverseLoader(new TimelineFactory());
            Assert.That(
                loader.TryLoad(scenario, new EntityIdFactory(), out var universe, out _, out _),
                Is.True);
            Assert.That(universe.MapGraph.IsFullyConnected(), Is.True);
            Assert.That(universe.ScenarioId, Is.EqualTo(GalaxyGenerator.ScenarioId));

            Object.DestroyImmediate(scenario);
            DestroyPools(pools, catalog, settings, shape);
        }

        [Test]
        public void TryCreateScenario_ProducesConnectedRoutes_AndHomeColonyPlanet()
        {
            var pools = CreatePools(out var catalog, out var settings, out var shape);
            var generator = new GalaxyGenerator(catalog, pools.Planets, pools.Systems);

            Assert.That(
                generator.TryCreateScenario(
                    RunSeed.FromValue(42),
                    GalaxyCatalogSO.DefaultSizeId,
                    GalaxyCatalogSO.DefaultShapeId,
                    settlementCatalog: null,
                    out var scenario),
                Is.True);

            Assert.That(scenario.Routes.Count, Is.GreaterThanOrEqualTo(scenario.Nodes.Count - 1));
            Assert.That(IsConnected(scenario), Is.True);

            var homeId = scenario.StartingEmpire.HomeSystemId;
            MapNodeDefinition homeNode = null;
            for (var i = 0; i < scenario.Nodes.Count; i++)
            {
                if (scenario.Nodes[i].Id == homeId)
                {
                    homeNode = scenario.Nodes[i];
                    break;
                }
            }

            Assert.That(homeNode, Is.Not.Null);
            Assert.That(homeNode.Kind, Is.EqualTo(MapNodeKind.System));
            Assert.That(homeNode.PlanetIds.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(homeNode.DisplayName, Is.Not.Empty);
            Assert.That(
                homeNode.DisplayName,
                Is.EqualTo("Helios Reach").Or.EqualTo("Ember Expanse"));

            var homePlanetId = scenario.StartingEmpire.StartingSettlements[0].PlanetId;
            Assert.That(homePlanetId, Is.EqualTo(homeNode.PlanetIds[0]));

            var foundHomePlanet = false;
            for (var i = 0; i < scenario.Planets.Count; i++)
            {
                if (scenario.Planets[i].Id == homePlanetId)
                {
                    foundHomePlanet = true;
                    Assert.That(scenario.Planets[i].SystemId, Is.EqualTo(homeId));
                    Assert.That(scenario.Planets[i].FootprintBudget, Is.GreaterThanOrEqualTo(8));
                    // Home picks only from footprint ≥ 8 templates (Temperate World in this pool).
                    Assert.That(scenario.Planets[i].DisplayName, Is.EqualTo("Temperate World"));
                    break;
                }
            }

            Assert.That(foundHomePlanet, Is.True);

            Object.DestroyImmediate(scenario);
            DestroyPools(pools, catalog, settings, shape);
        }

        [Test]
        public void TryCreateScenario_AppliesPlanetTemplateModifiers()
        {
            var pools = CreatePools(out var catalog, out var settings, out var shape);
            // Single industrial template so every planet must inherit its modifier.
            pools.Planets.SetContent(new[]
            {
                new PlanetTemplate(
                    "Forge",
                    "Hot.",
                    footprintBudget: 10,
                    new PlanetModifiers(1.5f, 1f, 1f, 1f)),
            });
            var generator = new GalaxyGenerator(catalog, pools.Planets, pools.Systems);
            Assert.That(
                generator.TryCreateScenario(
                    RunSeed.FromValue(9),
                    GalaxyCatalogSO.DefaultSizeId,
                    GalaxyCatalogSO.DefaultShapeId,
                    settlementCatalog: null,
                    out var scenario),
                Is.True);

            Assert.That(scenario.Planets.Count, Is.GreaterThan(0));
            for (var i = 0; i < scenario.Planets.Count; i++)
            {
                Assert.That(scenario.Planets[i].DisplayName, Is.EqualTo("Forge"));
                Assert.That(
                    scenario.Planets[i].Modifiers.Multiplier(ResourceKind.Industry),
                    Is.EqualTo(1.5f).Within(0.001f));
            }

            Object.DestroyImmediate(scenario);
            DestroyPools(pools, catalog, settings, shape);
        }

        private static TemplatePools CreatePools(
            out GalaxyCatalogSO catalog,
            out GenerationSettingsSO settings,
            out CircleGalaxyShapeSO shape)
        {
            settings = ScriptableObject.CreateInstance<GenerationSettingsSO>();
            shape = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            catalog = ScriptableObject.CreateInstance<GalaxyCatalogSO>();
            catalog.SetContent(
                new List<GalaxySizeOption> { new(GalaxySizeId.Medium, "Medium", settings) },
                new List<GalaxyShapeOption> { new(GalaxyShapeId.Circle, "Circle", shape) });

            var planets = ScriptableObject.CreateInstance<PlanetDatabaseSO>();
            planets.SetContent(new[]
            {
                new PlanetTemplate("Temperate World", "Balanced.", 10),
                new PlanetTemplate(
                    "Barren Rock",
                    "Tight.",
                    6,
                    new PlanetModifiers(0.8f, 1.1f, 1f, 1f)),
            });
            var systems = ScriptableObject.CreateInstance<StarSystemDatabaseSO>();
            systems.SetContent(new[]
            {
                new StarSystemTemplate("Helios Reach"),
                new StarSystemTemplate("Ember Expanse"),
            });
            return new TemplatePools(planets, systems);
        }

        private static void DestroyPools(
            TemplatePools pools,
            GalaxyCatalogSO catalog,
            GenerationSettingsSO settings,
            CircleGalaxyShapeSO shape)
        {
            Object.DestroyImmediate(pools.Planets);
            Object.DestroyImmediate(pools.Systems);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(settings);
            Object.DestroyImmediate(shape);
        }

        private static bool IsConnected(ScenarioDefinition scenario)
        {
            if (scenario.Nodes == null || scenario.Nodes.Count == 0)
            {
                return false;
            }

            var adjacency = new Dictionary<ContentId, List<ContentId>>();
            for (var i = 0; i < scenario.Nodes.Count; i++)
            {
                adjacency[scenario.Nodes[i].Id] = new List<ContentId>();
            }

            for (var i = 0; i < scenario.Routes.Count; i++)
            {
                var route = scenario.Routes[i];
                if (!adjacency.ContainsKey(route.NodeAId) || !adjacency.ContainsKey(route.NodeBId))
                {
                    return false;
                }

                adjacency[route.NodeAId].Add(route.NodeBId);
                adjacency[route.NodeBId].Add(route.NodeAId);
            }

            var start = scenario.Nodes[0].Id;
            var seen = new HashSet<ContentId> { start };
            var queue = new Queue<ContentId>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = adjacency[current];
                for (var i = 0; i < neighbors.Count; i++)
                {
                    if (seen.Add(neighbors[i]))
                    {
                        queue.Enqueue(neighbors[i]);
                    }
                }
            }

            return seen.Count == scenario.Nodes.Count;
        }

        private sealed class TemplatePools
        {
            public PlanetDatabaseSO Planets { get; }

            public StarSystemDatabaseSO Systems { get; }

            public TemplatePools(PlanetDatabaseSO planets, StarSystemDatabaseSO systems)
            {
                Planets = planets;
                Systems = systems;
            }
        }
    }
}
