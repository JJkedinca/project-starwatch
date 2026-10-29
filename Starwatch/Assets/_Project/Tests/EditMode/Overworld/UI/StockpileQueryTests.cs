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
            var run = PlaceIndustryOnly(stack, 103);
            var query = CreateQuery(stack);
            var industryRate = IndustryRate(stack, run);

            var industry = query.Describe(run).Get(ResourceKind.Industry);

            Assert.That(industry.Amount, Is.EqualTo(run.Timeline.Resources.Get(ResourceKind.Industry)));
            Assert.That(industry.GrossPerSecond, Is.EqualTo(industryRate).Within(0.001f));
            Assert.That(industry.NetPerSecond, Is.EqualTo(industryRate).Within(0.001f));
            Assert.That(industry.State, Is.EqualTo(StockpileState.Gaining));
        }

        [Test]
        public void Describe_MarksEnergyRunningOutWhenStockLastsUnderAMinute()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustryOnly(stack, 104);
            var query = CreateQuery(stack);
            var energy = query.Describe(run).Get(ResourceKind.Energy);
            var seconds = energy.Amount / -energy.NetPerSecond;

            Assert.That(energy.NetPerSecond, Is.LessThan(0f));
            Assert.That(energy.SecondsUntilEmpty, Is.EqualTo(seconds).Within(0.01f));
            Assert.That(energy.SecondsUntilEmpty, Is.LessThan(60f));
            Assert.That(energy.State, Is.EqualTo(StockpileState.RunningOut));
        }

        [Test]
        public void Describe_MarksEnergyDrainingWhenStockLastsOverAMinute()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustryOnly(stack, 105);
            run.Timeline.Resources.Grant(new StartingResources(energy: 200));
            var query = CreateQuery(stack);
            var energy = query.Describe(run).Get(ResourceKind.Energy);

            Assert.That(energy.NetPerSecond, Is.LessThan(0f));
            Assert.That(energy.SecondsUntilEmpty, Is.GreaterThan(60f));
            Assert.That(energy.State, Is.EqualTo(StockpileState.Draining));
        }

        [Test]
        public void Describe_MarksEnergyStalledWhenUpkeepCannotBePaid()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = PlaceIndustryOnly(stack, 106);
            var energyStock = run.Timeline.Resources.Get(ResourceKind.Energy);
            Assert.That(
                run.Timeline.Resources.TrySpend(new[] { new ResourceAmount(ResourceKind.Energy, energyStock) }),
                Is.True);
            var query = CreateQuery(stack);

            var model = query.Describe(run);
            var energy = model.Get(ResourceKind.Energy);
            var industry = model.Get(ResourceKind.Industry);

            Assert.That(model.IsEmpireStalled, Is.True);
            Assert.That(energy.Amount, Is.EqualTo(0));
            Assert.That(energy.State, Is.EqualTo(StockpileState.Stalled));
            Assert.That(energy.NetPerSecond, Is.EqualTo(-energy.UpkeepPerSecond));
            Assert.That(industry.State, Is.EqualTo(StockpileState.Stalled));
            Assert.That(industry.GrossPerSecond, Is.GreaterThan(0f));
            Assert.That(industry.NetPerSecond, Is.EqualTo(0f));
        }

        private static RunState PlaceIndustryOnly(OverworldTestContext stack, int seed)
        {
            var run = RunTestFixture.CreateUnsettledRun(stack.Manager, seed);
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(
                stack.BuildingService.Place(run, home, ContentId.From("industry-works")).Success,
                Is.True);
            return run;
        }

        private static float IndustryRate(OverworldTestContext stack, RunState run)
        {
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.ObjectDatabase.TryGetPlanet(home.Value, out var planet), Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            return new ProductionService(stack.ObjectDatabase).Quote(colony, planet).Output.Get(ResourceKind.Industry);
        }

        private static StockpileQuery CreateQuery(OverworldTestContext stack = null)
        {
            var database = stack != null
                ? stack.ObjectDatabase
                : RunTestFixture.CreateObjectDatabase();
            return new StockpileQuery(database, new ProductionService(database));
        }
    }
}
