using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star system presenter against a fake view: open and return to the map.
    /// </summary>
    public sealed class StarSystemPresenterTests
    {
        [Test]
        public void EmptySystem_KeepsItsStatusAcrossPresenterRestart()
        {
            var objectDatabase = RunTestFixture.CreateObjectDatabase();
            var homeId = ContentId.From("empty-scenario-home");
            var emptySystemId = ContentId.From("empty-scenario-system");
            var homePlanetId = ContentId.From("empty-scenario-home-planet");
            const string scenarioId = "empty-neighbor-scenario";
            var scenario = ScriptableObject.CreateInstance<ScenarioDefinition>();
            scenario.Configure(
                scenarioId,
                new StartingEmpireDefinition(homeId, ContentId.From("starter-screen"), starterShipCount: 1),
                new[]
                {
                    new MapNodeDefinition(homeId, MapNodeKind.System, UnityEngine.Vector2.zero, new[] { homePlanetId }),
                    new MapNodeDefinition(emptySystemId, MapNodeKind.System, UnityEngine.Vector2.one, System.Array.Empty<ContentId>()),
                },
                new[] { new MapRouteDefinition(homeId, emptySystemId, 5f) },
                new[]
                {
                    new PlanetDefinition(homePlanetId, homeId, "Home", string.Empty, footprintBudget: 10),
                });
            objectDatabase.RegisterScenario(scenario);

            var stack = RunTestFixture.CreateOverworldStack(objectDatabase);
            var run = stack.Manager.CreateRun(scenarioId, RunSeed.FromValue(33));
            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, emptySystemId);
            stack.ExplorationService.Explore(run, ObserverId.Player, emptySystemId);
            var selection = new OverworldSelection();
            var status = new OverworldStatus();
            var map = new FakeStarMapView();
            var system = new FakeStarSystemView();
            var mapPresenter = CreateMap(map, selection, status, stack);
            var systemPresenter = CreateSystem(system, selection, status, stack);
            systemPresenter.Start();
            mapPresenter.Start();

            map.ClickNode(emptySystemId);

            var posted = status.Text;
            Assert.That(posted, Is.Not.Empty);
            Assert.That(posted, Is.Not.EqualTo(OverworldStatus.kNoRun));
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
        public void ForeignPlanetScan_DoesNotRefreshTheSystem()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 58);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, new OverworldStatus(), stack);
            presenter.Start();
            var shown = view.Model;

            stack.Events.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.From("rival"),
                RunTestFixture.HomePlanet(run),
                MapKnowledgeKind.PlanetScanned));

            Assert.That(view.Model, Is.SameAs(shown));
            presenter.Dispose();
        }

        [Test]
        public void Back_ReturnsToTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 50);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            var status = new OverworldStatus();
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, status, stack);
            presenter.Start();

            view.ClickBack();

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(view.Model.IsOpen, Is.False);
            Assert.That(status.Toasts, Is.Empty);
            presenter.Dispose();
        }

        [Test]
        public void FleetClick_SelectsAFleetAtTheOpenSystem()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 62);
            var home = RunTestFixture.HomeSystem(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, RunTestFixture.FirstNeighbor(run), out _),
                Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(home);
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, new OverworldStatus(), stack);
            presenter.Start();

            view.ClickFleet(fleetId);

            Assert.That(selection.SelectedFleetId, Is.EqualTo(fleetId));
            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(view.Model.Fleets, Has.Count.EqualTo(1));
            Assert.That(view.Model.Fleets[0].FleetId, Is.EqualTo(fleetId));
            Assert.That(view.Model.Fleets[0].IsSelected, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesFleetSpeed()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 87);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            var view = new FakeStarSystemView();
            var presenter = CreateSystem(view, selection, new OverworldStatus(), stack);
            presenter.Start();

            Assert.That(view.Model.Fleets[0].TravelSpeed, Is.EqualTo(1f).Within(0.001f));

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-travel")), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-travel")));

            Assert.That(view.Model.Fleets[0].TravelSpeed, Is.EqualTo(1.25f).Within(0.001f));
            presenter.Dispose();
        }

        private static StarMapPresenter CreateMap(
            FakeStarMapView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            var database = stack.ObjectDatabase;
            var fleetListQuery = new FleetListQuery(database, stack.Effects, stack.Clock);
            return new StarMapPresenter(
                view,
                selection,
                status,
                stack.Manager,
                stack.TravelService,
                new StarMapQuery(stack.VisibilityService, stack.TravelService, fleetListQuery, database, stack.Clock),
                new StarSystemQuery(stack.VisibilityService, database, fleetListQuery),
                stack.Events);
        }

        private static StarSystemPresenter CreateSystem(
            FakeStarSystemView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            var database = stack.ObjectDatabase;
            return new StarSystemPresenter(
                view,
                selection,
                stack.Manager,
                new StarSystemQuery(
                    stack.VisibilityService,
                    database,
                    new FleetListQuery(database, stack.Effects, stack.Clock)),
                stack.Events);
        }

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected;

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

            public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
            {
            }

            public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
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

            public event Action<EntityId> FleetSelected;

            public StarSystemModel Model { get; private set; } = StarSystemModel.Closed;

            public void Apply(StarSystemModel model)
            {
                Model = model;
            }

            public void ClickBack()
            {
                BackRequested?.Invoke();
            }

            public void ClickFleet(EntityId fleetId)
            {
                FleetSelected?.Invoke(fleetId);
            }
        }
    }
}
