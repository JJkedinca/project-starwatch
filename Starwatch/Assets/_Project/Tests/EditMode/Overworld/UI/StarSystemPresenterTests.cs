using System;
using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star system presenter against a fake view: open, scan, and return to the map.
    /// </summary>
    public sealed class StarSystemPresenterTests
    {
        [Test]
        public void EmptySystem_KeepsItsStatusAcrossPresenterRestart()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(33));
            var belt = ContentId.From("asteroid-belt");
            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.Exploration.Explore(run, ObserverId.Player, belt);
            var selection = new OverworldSelection();
            var status = new OverworldStatus();
            var map = new FakeStarMapView();
            var system = new FakeStarSystemView();
            var mapPresenter = CreateMap(map, selection, status, stack);
            var systemPresenter = CreateSystem(system, selection, status, stack);
            systemPresenter.Start();
            mapPresenter.Start();

            map.ClickNode(belt);

            var posted = status.Text;
            Assert.That(posted, Is.Not.EqualTo(OverworldStatus.kMapHint));
            Assert.That(system.Model.IsOpen, Is.True);
            Assert.That(system.Model.Planets, Is.Empty);

            mapPresenter.Dispose();
            systemPresenter.Dispose();
            mapPresenter = CreateMap(map, selection, status, stack);
            systemPresenter = CreateSystem(system, selection, status, stack);
            systemPresenter.Start();
            mapPresenter.Start();

            Assert.That(status.Text, Is.EqualTo(posted));
            Assert.That(system.Model.Planets, Is.Empty);
            Assert.That(selection.InStarSystem, Is.True);
            mapPresenter.Dispose();
            systemPresenter.Dispose();
        }

        [Test]
        public void Scan_PostsTheScannedPlanetAndKeepsTheSystemOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(49));
            var prime = ContentId.From("home-prime");
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(prime);
            var status = new OverworldStatus();
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, status, stack);
            presenter.Start();
            var beforeScan = status.Text;

            view.ClickScan(prime);

            Assert.That(status.Text, Is.Not.EqualTo(beforeScan));
            Assert.That(status.Text, Is.Not.Empty);
            Assert.That(view.Model.IsOpen, Is.True);
            Assert.That(view.Model.ShowPlanetDetail, Is.True);
            Assert.That(view.Model.PlanetDetail.PlanetId, Is.EqualTo(prime));
            Assert.That(view.Model.PlanetDetail.CanScan, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void ForeignPlanetScan_DoesNotRefreshTheSystem()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(58));
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            var shown = view.Model;

            stack.Events.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.From("rival"),
                ContentId.From("home-prime"),
                MapKnowledgeKind.PlanetScanned));

            Assert.That(view.Model, Is.SameAs(shown));
            presenter.Dispose();
        }

        [Test]
        public void Back_ReturnsToTheMapHint()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(50));
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            var status = new OverworldStatus();
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, status, stack);
            presenter.Start();

            view.ClickBack();

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(view.Model.IsOpen, Is.False);
            Assert.That(status.Text, Is.EqualTo(OverworldStatus.kMapHint));
            presenter.Dispose();
        }

        private static StarMapPresenter CreateMap(
            FakeStarMapView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            return new StarMapPresenter(
                view,
                selection,
                status,
                stack.Manager,
                stack.Travel,
                new StarMapQuery(stack.Visibility, stack.Travel),
                new StarSystemQuery(stack.Visibility, RunTestFixture.CreateObjectDatabase()),
                stack.Events);
        }

        private static StarSystemPresenter CreateSystem(
            FakeStarSystemView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            return new StarSystemPresenter(
                view,
                selection,
                status,
                stack.Manager,
                stack.Sensor,
                new StarSystemQuery(stack.Visibility, RunTestFixture.CreateObjectDatabase()),
                stack.Events);
        }

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected;

            public event Action TravelRequested
            {
                add { }
                remove { }
            }

            public void Apply(StarMapModel model)
            {
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

            public void ClickNode(ContentId nodeId)
            {
                NodeSelected?.Invoke(nodeId);
            }
        }

        private sealed class FakeStarSystemView : IStarSystemView
        {
            public event Action BackRequested;

            public event Action<ContentId> PlanetSelected
            {
                add { }
                remove { }
            }

            public event Action<ContentId> ScanRequested;

            public StarSystemModel Model { get; private set; } = StarSystemModel.Closed;

            public void Apply(StarSystemModel model)
            {
                Model = model;
            }

            public void ClickBack()
            {
                BackRequested?.Invoke();
            }

            public void ClickScan(ContentId planetId)
            {
                ScanRequested?.Invoke(planetId);
            }
        }
    }
}
