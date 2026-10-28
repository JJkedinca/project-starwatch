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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 101);
            var database = stack.ObjectDatabase;
            var productionService = new ProductionService(database);
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.BuildingService.Place(run, home, ContentId.From("industry-works")).Success, Is.True);

            var quote = EmpireProductionQuote.Build(run, database, productionService, includeLines: true);

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(2f));
            Assert.That(quote.Upkeep.Get(ResourceKind.Energy), Is.EqualTo(1f));
            Assert.That(quote.Lines, Has.Count.EqualTo(1));
            Assert.That(quote.Lines[0].PlanetId, Is.EqualTo(home));
            Assert.That(quote.Lines[0].Output.Get(ResourceKind.Industry), Is.EqualTo(2f));
        }

        [Test]
        public void Build_WithoutLines_LeavesTheListEmpty()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 102);
            var database = stack.ObjectDatabase;
            Assert.That(
                stack.BuildingService.Place(run, RunTestFixture.HomePlanet(run), ContentId.From("industry-works")).Success,
                Is.True);

            var quote = EmpireProductionQuote.Build(run, database, new ProductionService(database));

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(2f));
            Assert.That(quote.Lines, Is.Empty);
        }
    }
}
