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
        public void TryGetScenario_RebuildsAfterRuntimeScenarioIsDestroyed()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            Assert.That(database.TryGetScenario("authored-vertical-slice", out var first), Is.True);

            Object.DestroyImmediate(first);

            Assert.That(database.TryGetScenario("authored-vertical-slice", out var rebuilt), Is.True);
            Assert.That(rebuilt.ScenarioId, Is.EqualTo("authored-vertical-slice"));

            Object.DestroyImmediate(rebuilt);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void TryGetScenario_RebuildsWhenTheIndexWasCleared()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            Assert.That(database.TryGetScenario("authored-vertical-slice", out _), Is.True);

            database.ResetBuiltIndex();

            Assert.That(database.TryGetScenario("authored-vertical-slice", out var rebuilt), Is.True);
            Assert.That(rebuilt.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(database.Scenarios.ContainsKey("authored-vertical-slice"), Is.True);

            Object.DestroyImmediate(rebuilt);
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
