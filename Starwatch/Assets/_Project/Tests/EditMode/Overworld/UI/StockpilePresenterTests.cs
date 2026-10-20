using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Stockpile bar against a fake view. Resource and settlement events refresh rates.
    /// </summary>
    public sealed class StockpilePresenterTests
    {
        [Test]
        public void Describe_ReadsTheEmpireStockpileWithoutARun()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(63));
            var query = CreateQuery();

            var stockpile = query.Describe(run);
            var missing = query.Describe(null);

            Assert.That(stockpile.Get(ResourceKind.Industry).Amount, Is.EqualTo(80));
            Assert.That(stockpile.Get(ResourceKind.Energy).Amount, Is.EqualTo(40));
            Assert.That(stockpile.Get(ResourceKind.Strategic).Amount, Is.EqualTo(0));
            Assert.That(stockpile.Research.SciencePerSecond, Is.EqualTo(0f));
            Assert.That(missing.Entries, Is.Empty);
        }

        [Test]
        public void ResourceChanged_UpdatesTheBarOnTheStarMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(64));
            var view = new FakeStockpileView();
            var presenter = CreatePresenter(view, stack);
            presenter.Start();

            run.Timeline.Resources.Add(ResourceKind.Industry, 3);
            stack.Events.Publish(new ResourceChangedEvent(GameTime.Zero));

            Assert.That(view.Model.Get(ResourceKind.Industry).Amount, Is.EqualTo(83));
            presenter.Dispose();
        }

        [Test]
        public void SettlementFounded_UpdatesTheBar()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(65),
                RunTestFixture.UnsettledVerticalSlice());
            var view = new FakeStockpileView();
            var presenter = CreatePresenter(view, stack);
            presenter.Start();

            stack.SettlementService.FoundOutpost(run, ObserverId.Player, ContentId.From("home-prime"));

            Assert.That(view.Model.Get(ResourceKind.Industry).Amount, Is.EqualTo(70));
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesRates()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(107));
            Assert.That(
                stack.BuildingService.Place(run, ContentId.From("home-prime"), ContentId.From("industry-works")).Success,
                Is.True);
            var view = new FakeStockpileView();
            var presenter = CreatePresenter(view, stack, CreateQuery(stack));
            presenter.Start();
            var before = view.Model.Get(ResourceKind.Industry).GrossPerSecond;

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-construction")), Is.True);
            stack.Effects.Rebuild(run, RunTestFixture.CreateObjectDatabase().ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-construction")));

            Assert.That(view.Model.Get(ResourceKind.Industry).GrossPerSecond, Is.GreaterThan(before));
            presenter.Dispose();
        }

        [Test]
        public void ScienceApplied_RefreshesActiveResearchProgress()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(108));
            Assert.That(run.Timeline.Research.Offers, Is.Not.Empty);
            Assert.That(stack.ResearchService.Start(run, run.Timeline.Research.Offers[0]).Success, Is.True);
            var view = new FakeStockpileView();
            var presenter = CreatePresenter(view, stack);
            presenter.Start();

            Assert.That(view.Model.Research.HasActive, Is.True);
            Assert.That(view.Model.Research.Points, Is.EqualTo(0f));

            stack.ResearchService.ApplyScience(run, 5f);

            Assert.That(view.Model.Research.Points, Is.EqualTo(5f));
            presenter.Dispose();
        }

        private static StockpilePresenter CreatePresenter(
            FakeStockpileView view,
            OverworldTestContext stack,
            StockpileQuery query = null)
        {
            return new StockpilePresenter(
                view,
                stack.Manager,
                query ?? CreateQuery(),
                stack.ResearchService,
                stack.Events);
        }

        private static StockpileQuery CreateQuery(OverworldTestContext stack = null)
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var productionService = stack != null
                ? new ProductionService(database, stack.Effects)
                : new ProductionService(database);
            return new StockpileQuery(database, productionService);
        }

        private sealed class FakeStockpileView : IStockpileView
        {
            public StockpileModel Model { get; private set; } = StockpileModel.Empty;

            public void Apply(StockpileModel model)
            {
                Model = model;
            }
        }
    }
}
