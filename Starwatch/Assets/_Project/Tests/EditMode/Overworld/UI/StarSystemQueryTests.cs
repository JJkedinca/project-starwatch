using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star system read model: visible planets and selection state.
    /// </summary>
    public sealed class StarSystemQueryTests
    {
        [Test]
        public void VisiblePlanets_IsEmptyForExploredLocationWithoutPlanets()
        {
            var objectDatabase = RunTestFixture.CreateObjectDatabase();
            RegisterEmptyNeighborScenario(objectDatabase, out var scenarioId, out var emptySystemId);
            var stack = RunTestFixture.CreateOverworldStack(objectDatabase);
            var run = stack.Manager.CreateRun(scenarioId, RunSeed.FromValue(44));
            var query = CreateQuery(stack, objectDatabase);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, emptySystemId);
            stack.ExplorationService.Explore(run, ObserverId.Player, emptySystemId);
            var selection = new OverworldSelection();
            selection.EnterSystem(emptySystemId);

            var model = query.Describe(run, selection);

            Assert.That(query.VisiblePlanets(run, RequireNode(run, emptySystemId)), Is.Empty);
            Assert.That(model.IsOpen, Is.True);
            Assert.That(model.Planets, Is.Empty);
        }

        [Test]
        public void Describe_MarksSelectedPlanetAndAuthoredName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 45);
            var database = stack.ObjectDatabase;
            var query = new StarSystemQuery(
                stack.VisibilityService,
                database,
                new FleetListQuery(database, stack.Effects, stack.Clock));
            var home = RequireNode(run, RunTestFixture.HomeSystem(run));
            var prime = RunTestFixture.HomePlanet(run);
            Assert.That(database.TryGetPlanet(prime.Value, out var planet), Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(home.Id);
            selection.SelectPlanet(prime);

            var row = query.PlanetState(run, home, prime, prime);
            var model = query.Describe(run, selection);

            Assert.That(row.DisplayName, Is.EqualTo(planet.DisplayName));
            Assert.That(row.IsSelected, Is.True);
            Assert.That(row.Visibility, Is.EqualTo(PlanetVisibility.SystemExplored));
            Assert.That(model.IsOpen, Is.True);
            Assert.That(
                model.Title,
                Is.EqualTo(OverworldNames.StarSystem(database, home.Id)));
            var expectedOrbit = -1;
            for (var i = 0; i < home.PlanetIds.Count; i++)
            {
                if (home.PlanetIds[i] == prime)
                {
                    expectedOrbit = i;
                    break;
                }
            }

            Assert.That(expectedOrbit, Is.GreaterThanOrEqualTo(0));
            Assert.That(model.OrbitSlotCount, Is.EqualTo(home.PlanetIds.Count));
            Assert.That(model.Planets, Has.Some.Matches<StarSystemPlanetModel>(
                planetRow => planetRow.PlanetId == prime
                    && planetRow.State.IsSelected
                    && planetRow.OrbitIndex == expectedOrbit));

            stack.SensorService.ScanPlanet(run, ObserverId.Player, prime);

            Assert.That(
                query.PlanetState(run, home, prime, default(ContentId)).Visibility,
                Is.EqualTo(PlanetVisibility.Scanned));
        }

        private static StarSystemQuery CreateQuery(OverworldTestContext stack, ObjectDatabaseSO database = null)
        {
            database = database ?? stack.ObjectDatabase;
            return new StarSystemQuery(
                stack.VisibilityService,
                database,
                new FleetListQuery(database, stack.Effects, stack.Clock));
        }

        private static void RegisterEmptyNeighborScenario(
            ObjectDatabaseSO objectDatabase,
            out string scenarioId,
            out ContentId emptySystemId)
        {
            var homeId = ContentId.From("empty-scenario-home");
            emptySystemId = ContentId.From("empty-scenario-system");
            var homePlanetId = ContentId.From("empty-scenario-home-planet");
            scenarioId = "empty-neighbor-scenario";
            var scenario = ScriptableObject.CreateInstance<ScenarioDefinition>();
            scenario.Configure(
                scenarioId,
                new StartingEmpireDefinition(homeId, ContentId.From("starter-screen"), starterShipCount: 1),
                new[]
                {
                    new MapNodeDefinition(homeId, MapNodeKind.System, Vector2.zero, new[] { homePlanetId }),
                    new MapNodeDefinition(emptySystemId, MapNodeKind.System, Vector2.one, System.Array.Empty<ContentId>()),
                },
                new[] { new MapRouteDefinition(homeId, emptySystemId, 5f) },
                new[]
                {
                    new PlanetDefinition(homePlanetId, homeId, "Home", string.Empty, footprintBudget: 10),
                });
            objectDatabase.RegisterScenario(scenario);
        }

        private static StarSystemQuery CreateQuery(OverworldTestContext stack)
        {
            return CreateQuery(stack, null);
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
