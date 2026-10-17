using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star system read model: visible planets and the selected planet's detail.
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
            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.Exploration.Explore(run, ObserverId.Player, belt);
            var selection = new OverworldSelection();
            selection.EnterSystem(belt);

            var model = query.Describe(run, selection);

            Assert.That(query.VisiblePlanets(run, RequireNode(run, belt)), Is.Empty);
            Assert.That(model.IsOpen, Is.True);
            Assert.That(model.Planets, Is.Empty);
            Assert.That(model.ShowPlanetDetail, Is.False);
        }

        [Test]
        public void Describe_UsesAuthoredNameAndScanCopy()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(45));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new StarSystemQuery(stack.Visibility, database);
            var home = RequireNode(run, ContentId.From("home-system"));
            var prime = ContentId.From("home-prime");
            Assert.That(database.TryGetPlanet(prime.Value, out var planet), Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(home.Id);
            selection.SelectPlanet(prime);

            var row = query.PlanetState(run, home, prime, prime);
            var ready = query.Describe(run, selection);

            Assert.That(row.DisplayName, Is.EqualTo(planet.DisplayName));
            Assert.That(row.IsSelected, Is.True);
            Assert.That(row.Visibility, Is.EqualTo(PlanetVisibility.SystemExplored));
            Assert.That(ready.ShowPlanetDetail, Is.True);
            Assert.That(ready.PlanetDetail.PlanetId, Is.EqualTo(prime));
            Assert.That(ready.PlanetDetail.DisplayName, Is.EqualTo(planet.DisplayName));
            Assert.That(ready.PlanetDetail.CanScan, Is.True);
            Assert.That(ready.PlanetDetail.Detail, Is.Not.EqualTo(planet.Summary));
            Assert.That(ready.Title, Is.EqualTo(home.Id.Value));

            stack.Sensor.ScanPlanet(run, ObserverId.Player, prime);
            var scanned = query.Describe(run, selection);

            Assert.That(scanned.PlanetDetail.Detail, Is.EqualTo(planet.Summary));
            Assert.That(scanned.PlanetDetail.CanScan, Is.False);
            Assert.That(
                query.PlanetState(run, home, prime, default(ContentId)).Visibility,
                Is.EqualTo(PlanetVisibility.Scanned));
        }

        private static StarSystemQuery CreateQuery(OverworldTestContext stack)
        {
            return new StarSystemQuery(stack.Visibility, RunTestFixture.CreateObjectDatabase());
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
