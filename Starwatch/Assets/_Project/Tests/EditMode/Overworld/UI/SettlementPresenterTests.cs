using System;
using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Settlement;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Settlement presenter against a fake view. Resource ticks rebuild only this panel.
    /// </summary>
    public sealed class SettlementPresenterTests
    {
        [Test]
        public void FoundOutpost_UpdatesThePanelAndPostsSuccess()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(34));
            var prime = ContentId.From("home-prime");
            run.Timeline.TryRemoveColony(prime);
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(prime);
            var status = new OverworldStatus();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            var beforeFounding = status.Text;

            view.ClickFoundOutpost();

            Assert.That(status.Text, Is.Not.EqualTo(beforeFounding));
            Assert.That(status.Text, Is.Not.Empty);
            Assert.That(run.Timeline.TryGetColony(prime, out var colony), Is.True);
            Assert.That(colony.Presence, Is.EqualTo(ColonyPresence.Outpost));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(70));
            Assert.That(view.Model.Resources.GetWhole(ResourceKind.Industry), Is.EqualTo(70));
            Assert.That(view.Model.Commitment.CanFoundOutpost, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void ResourceChanged_RebuildsTheSettlementPanelOnly()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(51));
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(ContentId.From("home-prime"));
            var map = new FakeStarMapView();
            var settlement = new FakeSettlementView();
            var mapPresenter = new StarMapPresenter(
                map,
                selection,
                new OverworldStatus(),
                stack.Manager,
                stack.Travel,
                new StarMapQuery(stack.Visibility, stack.Travel),
                new StarSystemQuery(stack.Visibility, RunTestFixture.CreateObjectDatabase()),
                stack.Events);
            var settlementPresenter = CreatePresenter(settlement, selection, new OverworldStatus(), stack);
            mapPresenter.Start();
            settlementPresenter.Start();
            var mapApplies = map.ApplyCount;

            run.Timeline.Resources.Add(ResourceKind.Industry, 3);
            stack.Events.Publish(new ResourceChangedEvent(GameTime.Zero));

            Assert.That(map.ApplyCount, Is.EqualTo(mapApplies));
            Assert.That(settlement.Model.Resources.GetWhole(ResourceKind.Industry), Is.EqualTo(83));
            mapPresenter.Dispose();
            settlementPresenter.Dispose();
        }

        [Test]
        public void ResourceChanged_SkipsThePanelWhileTheStarMapIsShowing()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(59));
            var selection = new OverworldSelection();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            run.Timeline.Resources.Add(ResourceKind.Industry, 3);
            stack.Events.Publish(new ResourceChangedEvent(GameTime.Zero));

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.Model.ShowPanel, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Scan_RevealsTheSelectedPlanetWithoutReselecting()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(61));
            var prime = ContentId.From("home-prime");
            run.Timeline.TryRemoveColony(prime);
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(prime);
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            var applies = view.ApplyCount;
            var beforeScan = view.Model.Development;
            var database = RunTestFixture.CreateObjectDatabase();
            database.TryGetPlanet(prime.Value, out var planet);
            var production = new ProductionService(database);

            Assert.That(view.Model.Economy.IsRevealed, Is.False);
            stack.Sensor.ScanPlanet(run, ObserverId.Player, prime);

            Assert.That(selection.SelectedPlanetId, Is.EqualTo(prime));
            Assert.That(view.ApplyCount, Is.GreaterThan(applies));
            Assert.That(view.Model.Development, Is.Not.EqualTo(beforeScan));
            Assert.That(view.Model.Economy.IsRevealed, Is.True);
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Industry),
                Is.EqualTo(production.ModifiedBase(planet).Get(ResourceKind.Industry)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Energy),
                Is.EqualTo(production.ModifiedBase(planet).Get(ResourceKind.Energy)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Science),
                Is.EqualTo(production.ModifiedBase(planet).Get(ResourceKind.Science)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Strategic),
                Is.EqualTo(production.ModifiedBase(planet).Get(ResourceKind.Strategic)));
            Assert.That(view.Model.Economy.Rate(ResourceKind.Industry), Is.EqualTo(0f));
            presenter.Dispose();
        }

        [Test]
        public void Commands_DoNothingWhileTheStarMapIsShowing()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(52));
            var status = new OverworldStatus();
            var selection = new OverworldSelection();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            status.Post(OverworldStatus.kMapHint);
            var beforeCommand = status.Text;

            view.ClickFoundOutpost();

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(status.Text, Is.EqualTo(beforeCommand));
            Assert.That(view.Model.ShowPanel, Is.False);
            presenter.Dispose();
        }

        private static SettlementPresenter CreatePresenter(
            FakeSettlementView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            var database = RunTestFixture.CreateObjectDatabase();
            return new SettlementPresenter(
                view,
                selection,
                status,
                stack.Manager,
                stack.Settlement,
                stack.Buildings,
                new SettlementQuery(database, new ProductionService(database)),
                stack.Events);
        }

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected
            {
                add { }
                remove { }
            }

            public event Action TravelRequested
            {
                add { }
                remove { }
            }

            public int ApplyCount { get; private set; }

            public void Apply(StarMapModel model)
            {
                ApplyCount++;
            }

            public void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes)
            {
            }

            public void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes)
            {
            }

            public void ApplyCard(StarMapCardModel card)
            {
            }
        }

        private sealed class FakeSettlementView : ISettlementView
        {
            public event Action FoundOutpostRequested;

            public event Action FoundColonyRequested
            {
                add { }
                remove { }
            }

            public event Action AbandonRequested
            {
                add { }
                remove { }
            }

            public event Action<ContentId> PlaceRequested
            {
                add { }
                remove { }
            }

            public event Action<ContentId> UpgradeRequested
            {
                add { }
                remove { }
            }

            public int ApplyCount { get; private set; }

            public SettlementPanelModel Model { get; private set; }

            public void Apply(SettlementPanelModel model)
            {
                ApplyCount++;
                Model = model;
            }

            public void ClickFoundOutpost()
            {
                FoundOutpostRequested?.Invoke();
            }
        }
    }
}
