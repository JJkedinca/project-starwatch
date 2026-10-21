using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

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
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(44));
            var query = CreateQuery(stack);
            var belt = ContentId.From("asteroid-belt");
            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.ExplorationService.Explore(run, ObserverId.Player, belt);
            var selection = new OverworldSelection();
            selection.EnterSystem(belt);

            var model = query.Describe(run, selection);

            Assert.That(query.VisiblePlanets(run, RequireNode(run, belt)), Is.Empty);
            Assert.That(model.IsOpen, Is.True);
            Assert.That(model.Planets, Is.Empty);
        }

        [Test]
        public void Describe_MarksSelectedPlanetAndAuthoredName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(45));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new StarSystemQuery(
                stack.VisibilityService,
                database,
                new FleetListQuery(database, stack.Effects, stack.Clock));
            var home = RequireNode(run, ContentId.From("home-system"));
            var prime = ContentId.From("home-prime");
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
            Assert.That(model.Title, Is.EqualTo(home.Id.Value));
            Assert.That(model.Planets, Has.Some.Matches<StarSystemPlanetModel>(
                planetRow => planetRow.PlanetId == prime && planetRow.State.IsSelected));

            stack.SensorService.ScanPlanet(run, ObserverId.Player, prime);

            Assert.That(
                query.PlanetState(run, home, prime, default(ContentId)).Visibility,
                Is.EqualTo(PlanetVisibility.Scanned));
        }

        private static StarSystemQuery CreateQuery(OverworldTestContext stack)
        {
            var database = RunTestFixture.CreateObjectDatabase();
            return new StarSystemQuery(
                stack.VisibilityService,
                database,
                new FleetListQuery(database, stack.Effects, stack.Clock));
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
