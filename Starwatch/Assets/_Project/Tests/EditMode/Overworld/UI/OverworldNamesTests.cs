using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Shared overworld display labels.
    /// </summary>
    public sealed class OverworldNamesTests
    {
        [Test]
        public void FleetTitle_JoinsEveryStackLikeTheRoster()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(71));
            var database = RunTestFixture.CreateObjectDatabase();
            var home = ContentId.From("home-system");
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("line-ship"), 4, home, out var fleetId),
                Is.True);
            Assert.That(
                run.Timeline.Fleets.TryAddShips(fleetId, ContentId.From("starter-screen"), 2),
                Is.True);
            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(database.ShipCatalog.TryGet(ContentId.From("line-ship"), out var line), Is.True);
            Assert.That(database.ShipCatalog.TryGet(ContentId.From("starter-screen"), out var screen), Is.True);

            var title = OverworldNames.FleetTitle(database, fleet);

            Assert.That(title, Does.Contain("4 " + line.DisplayName));
            Assert.That(title, Does.Contain("2 " + screen.DisplayName));
            Assert.That(title, Does.Not.Contain(line.Id.Value));
        }

        [Test]
        public void Labels_PreferCatalogDisplayNames()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var forge = ContentId.From("forge-system");
            var prime = ContentId.From("home-prime");
            var researchId = ContentId.From("basic-travel");
            Assert.That(database.TryGetStarSystem(forge.Value, out var system), Is.True);
            Assert.That(database.TryGetPlanet(prime.Value, out var planet), Is.True);
            Assert.That(database.ResearchCatalog.TryGet(researchId, out var project), Is.True);

            Assert.That(OverworldNames.StarSystem(database, forge), Is.EqualTo(system.DisplayName));
            Assert.That(OverworldNames.Planet(database, prime), Is.EqualTo(planet.DisplayName));
            Assert.That(OverworldNames.Research(database, researchId), Is.EqualTo(project.DisplayName));
        }

        [Test]
        public void RemainingSeconds_ClampsPastArrivalsToZero()
        {
            var clock = new SimulationClock();
            clock.AdvanceSimulation(10f);

            Assert.That(
                OverworldNames.RemainingSeconds(clock, new GameTime(7d)),
                Is.EqualTo(0f));
            Assert.That(
                OverworldNames.RemainingSeconds(clock, new GameTime(12.5d)),
                Is.EqualTo(2.5f).Within(0.001f));
        }
    }
}
