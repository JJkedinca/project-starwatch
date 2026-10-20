using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Stockpile rates and warning states for the top bar.
    /// </summary>
    public sealed class StockpileQueryTests
    {
        [Test]
        public void Describe_WithoutARun_ReturnsEmpty()
        {
            var query = CreateQuery();

            var model = query.Describe(null);

            Assert.That(model.Entries, Is.Empty);
            Assert.That(model.Research.SciencePerSecond, Is.EqualTo(0f));
            Assert.That(model.IsEmpireStalled, Is.False);
        }

        [Test]
        public void Describe_MarksIndustryGainingAfterAnIndustryWorks()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustry(stack, 103);
            var query = CreateQuery();

            var industry = query.Describe(run).Get(ResourceKind.Industry);

            Assert.That(industry.Amount, Is.EqualTo(60));
            Assert.That(industry.GrossPerSecond, Is.EqualTo(2f));
            Assert.That(industry.NetPerSecond, Is.EqualTo(2f));
            Assert.That(industry.State, Is.EqualTo(StockpileState.Gaining));
        }

        [Test]
        public void Describe_MarksEnergyRunningOutWhenStockLastsUnderAMinute()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustry(stack, 104);
            var query = CreateQuery();

            var energy = query.Describe(run).Get(ResourceKind.Energy);

            Assert.That(energy.NetPerSecond, Is.EqualTo(-1f));
            Assert.That(energy.SecondsUntilEmpty, Is.EqualTo(40f).Within(0.01f));
            Assert.That(energy.State, Is.EqualTo(StockpileState.RunningOut));
        }

        [Test]
        public void Describe_MarksEnergyDrainingWhenStockLastsOverAMinute()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustry(stack, 105);
            run.Timeline.Resources.Grant(new StartingResources(energy: 80));
            var query = CreateQuery();

            var energy = query.Describe(run).Get(ResourceKind.Energy);

            Assert.That(energy.Amount, Is.EqualTo(120));
            Assert.That(energy.SecondsUntilEmpty, Is.EqualTo(120f).Within(0.01f));
            Assert.That(energy.State, Is.EqualTo(StockpileState.Draining));
        }

        [Test]
        public void Describe_MarksEnergyStalledWhenUpkeepCannotBePaid()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustry(stack, 106);
            Assert.That(
                run.Timeline.Resources.TrySpend(new[] { new ResourceAmount(ResourceKind.Energy, 40) }),
                Is.True);
            var query = CreateQuery();

            var model = query.Describe(run);
            var energy = model.Get(ResourceKind.Energy);

            Assert.That(model.IsEmpireStalled, Is.True);
            Assert.That(energy.Amount, Is.EqualTo(0));
            Assert.That(energy.State, Is.EqualTo(StockpileState.Stalled));
        }

        private static RunState PlaceIndustry(OverworldTestContext stack, int seed)
        {
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(seed));
            Assert.That(
                stack.BuildingService.Place(run, ContentId.From("home-prime"), ContentId.From("industry-works")).Success,
                Is.True);
            return run;
        }

        private static StockpileQuery CreateQuery()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            return new StockpileQuery(database, new ProductionService(database));
        }
    }
}
