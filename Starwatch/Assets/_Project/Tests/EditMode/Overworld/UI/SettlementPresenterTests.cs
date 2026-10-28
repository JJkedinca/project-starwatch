using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using ProjectStarwatch.Core.Generation;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
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
            var run = stack.Manager.CreateRun(
                GalaxyGenerator.ScenarioId,
                RunSeed.FromValue(34),
                RunTestFixture.UnsettledGeneratedEmpire());
            var prime = RunTestFixture.HomePlanet(run);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(prime);
            var status = new OverworldStatus();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();

            view.ClickFoundOutpost();

            // Founding success toasts are owned by OverworldToastPresenter (domain event).
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 51);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(RunTestFixture.HomePlanet(run));
            var map = new FakeStarMapView();
            var settlement = new FakeSettlementView();
            var database = stack.ObjectDatabase;
            var fleetListQuery = new FleetListQuery(database, stack.Effects, stack.Clock);
            var mapPresenter = new StarMapPresenter(
                map,
                selection,
                new OverworldStatus(),
                stack.Manager,
                stack.TravelService,
                new StarMapQuery(stack.VisibilityService, stack.TravelService, fleetListQuery, database, stack.Clock),
                new StarSystemQuery(stack.VisibilityService, database, fleetListQuery),
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 59);
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
            var run = stack.Manager.CreateRun(
                GalaxyGenerator.ScenarioId,
                RunSeed.FromValue(61),
                RunTestFixture.UnsettledGeneratedEmpire());
            var prime = RunTestFixture.HomePlanet(run);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(prime);
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            var applies = view.ApplyCount;
            var beforeScan = view.Model.Development;
            var database = stack.ObjectDatabase;
            database.TryGetPlanet(prime.Value, out var planet);
            var productionService = new ProductionService(database);

            Assert.That(view.Model.Economy.IsRevealed, Is.False);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, prime);

            Assert.That(selection.SelectedPlanetId, Is.EqualTo(prime));
            Assert.That(view.ApplyCount, Is.GreaterThan(applies));
            Assert.That(view.Model.Development, Is.Not.EqualTo(beforeScan));
            Assert.That(view.Model.Economy.IsRevealed, Is.True);
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Industry),
                Is.EqualTo(productionService.ModifiedBase(planet).Get(ResourceKind.Industry)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Energy),
                Is.EqualTo(productionService.ModifiedBase(planet).Get(ResourceKind.Energy)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Science),
                Is.EqualTo(productionService.ModifiedBase(planet).Get(ResourceKind.Science)));
            Assert.That(
                view.Model.Economy.Base.Get(ResourceKind.Strategic),
                Is.EqualTo(productionService.ModifiedBase(planet).Get(ResourceKind.Strategic)));
            Assert.That(view.Model.Economy.Rate(ResourceKind.Industry), Is.EqualTo(0f));
            presenter.Dispose();
        }

        [Test]
        public void Commands_DoNothingWhileTheStarMapIsShowing()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 52);
            var status = new OverworldStatus();
            var selection = new OverworldSelection();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            status.Post("keep this toast");
            var beforeCommand = status.Text;

            view.ClickFoundOutpost();

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(status.Text, Is.EqualTo(beforeCommand));
            Assert.That(view.Model.ShowPanel, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Scan_PostsDisplayNameAndClearsCanScan()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 49);
            var prime = RunTestFixture.HomePlanet(run);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(prime);
            var status = new OverworldStatus();
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            Assert.That(view.Model.Header.CanScan, Is.True);
            var database = stack.ObjectDatabase;
            Assert.That(database.TryGetPlanet(prime.Value, out var planet), Is.True);

            view.ClickScan();

            Assert.That(status.Text, Does.Contain(planet.DisplayName));
            Assert.That(status.Text, Does.Not.Contain(prime.Value));
            Assert.That(view.Model.Header.CanScan, Is.False);
            Assert.That(view.Model.Header.IsScanned, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesProduction()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 89);
            var prime = RunTestFixture.HomePlanet(run);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(prime);
            var view = new FakeSettlementView();
            var presenter = CreatePresenter(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            stack.SensorService.ScanPlanet(run, ObserverId.Player, prime);
            var before = view.Model.Economy.Production.Get(ResourceKind.Industry);

            Assert.That(before, Is.GreaterThan(0f));
            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-construction")), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-construction")));

            Assert.That(
                view.Model.Economy.Production.Get(ResourceKind.Industry),
                Is.EqualTo(before * 1.1f).Within(0.001f));
            presenter.Dispose();
        }

        private static SettlementPresenter CreatePresenter(
            FakeSettlementView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            var database = stack.ObjectDatabase;
            return new SettlementPresenter(
                view,
                selection,
                status,
                stack.Manager,
                stack.SettlementService,
                stack.BuildingService,
                stack.SensorService,
                database,
                new SettlementQuery(
                    database,
                    new ProductionService(database, stack.Effects),
                    stack.ShipProductionService),
                stack.Events);
        }

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected
            {
                add { }
                remove { }
            }

            public event Action<ContentId> TravelRequested
            {
                add { }
                remove { }
            }

            public event Action<ContentId> NodeHovered
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

            public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
            {
            }

            public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
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

            public event Action ScanRequested;

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

            public void ClickScan()
            {
                ScanRequested?.Invoke();
            }
        }
    }
}
