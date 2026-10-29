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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 71);
            var database = stack.ObjectDatabase;
            var home = RunTestFixture.HomeSystem(run);
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
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 42);
            var database = stack.ObjectDatabase;
            var systemId = RunTestFixture.FirstNeighbor(run);
            var planetId = RunTestFixture.HomePlanet(run);
            var researchId = ContentId.From("basic-travel");
            Assert.That(database.TryGetPlanet(planetId.Value, out var planet), Is.True);
            Assert.That(database.ResearchCatalog.TryGet(researchId, out var project), Is.True);

            Assert.That(database.TryGetStarSystemDisplayName(systemId.Value, out var systemName), Is.True);
            Assert.That(OverworldNames.StarSystem(database, systemId), Is.EqualTo(systemName));
            Assert.That(OverworldNames.Planet(database, planetId), Is.EqualTo(planet.DisplayName));
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
