using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for explore/scan affordances used by the map presenter.
    /// </summary>
    public sealed class ExplorationUiContractTests
    {
        [Test]
        public void CanExplore_TrueOnlyWhenFleetIsAtUnexploredNode()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(11));
            var forge = ContentId.From("forge-system");

            Assert.That(stack.ExplorationService.CanExplore(run, ObserverId.Player, forge), Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);

            Assert.That(stack.ExplorationService.CanExplore(run, ObserverId.Player, forge), Is.True);
            Assert.That(
                stack.ExplorationService.CanExplore(run, ObserverId.Player, ContentId.From("frontier-system")),
                Is.False);
        }

        [Test]
        public void ExploreThenScan_MatchesPresenterFlow()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(12));
            var forge = ContentId.From("forge-system");
            var planet = ContentId.From("forge-world");

            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, planet).Success, Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);
            var explored = stack.ExplorationService.Explore(run, ObserverId.Player, forge);
            Assert.That(explored.Success, Is.True);
            Assert.That(stack.ExplorationService.CanExplore(run, ObserverId.Player, forge), Is.False);

            var scanned = stack.SensorService.ScanPlanet(run, ObserverId.Player, planet);
            Assert.That(scanned.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.True);
        }
    }
}
