using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Generation;
using ProjectStarwatch.Core.Identifiers;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    public sealed class ObjectDatabaseSOTests
    {
        [Test]
        public void TryGetScenario_ReturnsRegisteredScenario()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var scenario = CreateGeneratedStandIn("gen-node-0-0");
            database.RegisterScenario(scenario);

            Assert.That(database.TryGetScenario(GalaxyGenerator.ScenarioId, out var found), Is.True);
            Assert.That(found, Is.SameAs(scenario));

            Object.DestroyImmediate(scenario);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void TryGetScenario_FailsAfterResetBuiltIndexUntilReRegistered()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var scenario = CreateGeneratedStandIn("gen-node-0-0");
            database.RegisterScenario(scenario);
            Assert.That(database.TryGetScenario(GalaxyGenerator.ScenarioId, out _), Is.True);

            database.ResetBuiltIndex();

            Assert.That(database.TryGetScenario(GalaxyGenerator.ScenarioId, out _), Is.False);

            var again = CreateGeneratedStandIn("gen-node-0-0");
            database.RegisterScenario(again);
            Assert.That(database.TryGetScenario(GalaxyGenerator.ScenarioId, out var found), Is.True);
            Assert.That(found, Is.SameAs(again));

            Object.DestroyImmediate(again);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void ListScenarioIds_IncludesGeneratedGalaxyWithoutRegistration()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var ids = database.ListScenarioIds();

            Assert.That(ids, Does.Contain(GalaxyGenerator.ScenarioId));
            Assert.That(database.TryGetScenario(GalaxyGenerator.ScenarioId, out _), Is.False);

            Object.DestroyImmediate(database);
        }

        [Test]
        public void RegisterScenario_ReplacingSameId_RemovesStalePlanetEntries()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var large = CreateGeneratedStandIn(
                "gen-node-99-0",
                "gen-node-99-1");
            database.RegisterScenario(large);

            Assert.That(database.TryGetPlanet("gen-node-99-0", out _), Is.True);
            Assert.That(database.TryGetPlanet("gen-node-99-1", out _), Is.True);

            var small = CreateGeneratedStandIn("gen-node-0-0");
            database.RegisterScenario(small);

            Assert.That(database.TryGetPlanet("gen-node-0-0", out _), Is.True);
            Assert.That(database.TryGetPlanet("gen-node-99-0", out _), Is.False);
            Assert.That(database.TryGetPlanet("gen-node-99-1", out _), Is.False);

            Object.DestroyImmediate(small);
            Object.DestroyImmediate(database);
        }

        private static ScenarioDefinition CreateGeneratedStandIn(params string[] planetIds)
        {
            var systemId = ContentId.From("gen-node-0");
            var planets = new PlanetDefinition[planetIds.Length];
            var planetContentIds = new ContentId[planetIds.Length];
            for (var i = 0; i < planetIds.Length; i++)
            {
                var planetId = ContentId.From(planetIds[i]);
                planetContentIds[i] = planetId;
                planets[i] = new PlanetDefinition(
                    planetId,
                    systemId,
                    displayName: planetIds[i],
                    summary: string.Empty,
                    footprintBudget: 10);
            }

            var scenario = ScriptableObject.CreateInstance<ScenarioDefinition>();
            scenario.Configure(
                GalaxyGenerator.ScenarioId,
                StartingEmpireDefinition.CreateDefault(),
                new[] { new MapNodeDefinition(systemId, MapNodeKind.System, Vector2.zero, planetContentIds) },
                System.Array.Empty<MapRouteDefinition>(),
                planets);
            return scenario;
        }
    }
}
