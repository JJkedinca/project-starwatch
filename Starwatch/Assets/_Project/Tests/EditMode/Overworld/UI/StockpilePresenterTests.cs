using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Stockpile bar against a fake view. Resource changes refresh it on the map and in a system.
    /// </summary>
    public sealed class StockpilePresenterTests
    {
        [Test]
        public void Describe_ReadsTheEmpireStockpileWithoutARun()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(63));
            var query = new StockpileQuery();

            var stockpile = query.Describe(run);
            var missing = query.Describe(null);

            Assert.That(stockpile.GetWhole(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(stockpile.GetWhole(ResourceKind.Energy), Is.EqualTo(40));
            Assert.That(stockpile.GetWhole(ResourceKind.Science), Is.EqualTo(0));
            Assert.That(stockpile.GetWhole(ResourceKind.Strategic), Is.EqualTo(0));
            Assert.That(missing.GetWhole(ResourceKind.Industry), Is.EqualTo(0));
        }

        [Test]
        public void ResourceChanged_UpdatesTheBarOnTheStarMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(64));
            var view = new FakeStockpileView();
            var presenter = new StockpilePresenter(view, stack.Manager, new StockpileQuery(), stack.Events);
            presenter.Start();

            run.Timeline.Resources.Add(ResourceKind.Industry, 3);
            stack.Events.Publish(new ResourceChangedEvent(GameTime.Zero));

            Assert.That(view.Stockpile.GetWhole(ResourceKind.Industry), Is.EqualTo(83));
            presenter.Dispose();
        }

        [Test]
        public void SettlementFounded_UpdatesTheBar()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(65));
            run.Timeline.TryRemoveColony(ContentId.From("home-prime"));
            var view = new FakeStockpileView();
            var presenter = new StockpilePresenter(view, stack.Manager, new StockpileQuery(), stack.Events);
            presenter.Start();

            stack.Settlement.FoundOutpost(run, ObserverId.Player, ContentId.From("home-prime"));

            Assert.That(view.Stockpile.GetWhole(ResourceKind.Industry), Is.EqualTo(70));
            presenter.Dispose();
        }

        private sealed class FakeStockpileView : IStockpileView
        {
            public ResourceInventory Stockpile { get; private set; }

            public void Apply(ResourceInventory stockpile)
            {
                Stockpile = stockpile;
            }
        }
    }
}
