using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Settlement
{
    /// <summary>
    /// Empire production quote shared by the economy tick and the stockpile bar.
    /// </summary>
    public sealed class EmpireProductionQuoteTests
    {
        [Test]
        public void Build_SumsColonyOutputAndUpkeep()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateUnsettledRun(stack.Manager, 101);
            var database = stack.ObjectDatabase;
            var productionService = new ProductionService(database);
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.BuildingService.Place(run, home, ContentId.From("industry-works")).Success, Is.True);
            Assert.That(database.TryGetPlanet(home.Value, out var planet), Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            var expected = productionService.Quote(colony, planet);

            var quote = EmpireProductionQuote.Build(run, database, productionService, includeLines: true);

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(expected.Output.Get(ResourceKind.Industry)));
            Assert.That(quote.Upkeep.Get(ResourceKind.Energy), Is.EqualTo(expected.Upkeep.Get(ResourceKind.Energy)));
            Assert.That(quote.Lines, Has.Count.EqualTo(1));
            Assert.That(quote.Lines[0].PlanetId, Is.EqualTo(home));
            Assert.That(
                quote.Lines[0].Output.Get(ResourceKind.Industry),
                Is.EqualTo(expected.Output.Get(ResourceKind.Industry)));
        }

        [Test]
        public void Build_WithoutLines_LeavesTheListEmpty()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateUnsettledRun(stack.Manager, 102);
            var database = stack.ObjectDatabase;
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(
                stack.BuildingService.Place(run, home, ContentId.From("industry-works")).Success,
                Is.True);
            Assert.That(database.TryGetPlanet(home.Value, out var planet), Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            var productionService = new ProductionService(database);
            var expected = productionService.Quote(colony, planet);

            var quote = EmpireProductionQuote.Build(run, database, productionService);

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(expected.Output.Get(ResourceKind.Industry)));
            Assert.That(quote.Lines, Is.Empty);
        }
    }
}
