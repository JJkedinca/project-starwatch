using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star map presenter against a fake view: enter, select, and travel.
    /// </summary>
    public sealed class StarMapPresenterTests
    {
        [Test]
        public void NodeClick_EntersAnExploredSystemAndSelectsAHiddenNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(46));
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            view.ClickNode(ContentId.From("home-system"));

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);

            selection.SelectPlanet(ContentId.From("home-prime"));
            view.ClickNode(ContentId.From("home-system"));

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);

            view.ClickNode(ContentId.From("forge-system"));

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(ContentId.From("forge-system")));
            presenter.Dispose();
        }

        [Test]
        public void Travel_PostsTravelingThenArrived()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(47));
            var selection = new OverworldSelection();
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            view.ClickNode(ContentId.From("forge-system"));
            var beforeTravel = status.Text;

            view.RightClickNode(ContentId.From("forge-system"));

            var whileTraveling = status.Text;
            Assert.That(whileTraveling, Is.Not.EqualTo(beforeTravel));
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.True);

            stack.Clock.SetPaused(false);
            var ticks = 0;
            while (run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
            {
                stack.Loop.Pump(1f / 30f);
                ticks++;
            }

            Assert.That(run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.False);
            Assert.That(status.Text, Is.Not.EqualTo(whileTraveling));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(ContentId.From("forge-system")), Is.True);
            presenter.Dispose();
        }

        [Test]
        public void PlanetScan_DoesNotRebuildTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(48));
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            stack.SensorService.ScanPlanet(run, ObserverId.Player, ContentId.From("home-prime"));

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.NodeApplyCount, Is.EqualTo(0));
            Assert.That(view.RouteApplyCount, Is.EqualTo(0));
            Assert.That(view.CardApplyCount, Is.EqualTo(0));
            presenter.Dispose();
        }

        [Test]
        public void FleetTravel_UpdatesOnlyTheEndpoints()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(53));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            view.ClickNode(forge);
            view.ClearPartial();

            view.RightClickNode(ContentId.From("forge-system"));

            Assert.That(view.ApplyCount, Is.EqualTo(1));
            Assert.That(view.RouteApplyCount, Is.GreaterThan(0));
            Assert.That(view.LastRoutes, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.InTransit));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.FleetMarkerApplyCount, Is.GreaterThan(0));
            Assert.That(view.LastFleetMarkers, Has.Some.Matches<FleetMarkerModel>(
                marker => marker.Kind == FleetMarkerKind.Transit
                    && marker.NodeId == home
                    && marker.DestinationNodeId == forge));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { home, forge }));
            Assert.That(run.Timeline.Fleets.HasTravelingFleetAt(home), Is.True);
            presenter.Dispose();
        }

        [Test]
        public void FleetTravel_ArrivalSnapsMarkerToDestination()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(103));
            var forge = ContentId.From("forge-system");
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectFleet(fleetId);

            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            Assert.That(view.LastFleetMarkers, Has.Some.Matches<FleetMarkerModel>(
                marker => marker.Kind == FleetMarkerKind.Transit));

            PumpUntilIdle(stack, run);

            Assert.That(view.LastFleetMarkers, Has.Some.Matches<FleetMarkerModel>(
                marker => marker.Kind == FleetMarkerKind.Parked
                    && marker.NodeId == forge
                    && marker.IsSelected));
            Assert.That(view.LastFleetMarkers, Has.None.Matches<FleetMarkerModel>(
                marker => marker.Kind == FleetMarkerKind.Transit));
            presenter.Dispose();
        }

        [Test]
        public void ArrivalAtExploredNode_RefreshesBothNeighborhoods()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(57));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var belt = ContentId.From("asteroid-belt");
            var choke = ContentId.From("choke-system");
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var outbound = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge);
            Assert.That(outbound.Success, Is.True);
            PumpUntilIdle(stack, run);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(home), Is.True);

            selection.SelectFleet(fleetId);
            view.ClickNode(choke);
            Assert.That(view.LastCard.HasArrival, Is.True);
            view.ClearPartial();
            var applies = view.ApplyCount;

            var returnTrip = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, home);
            Assert.That(returnTrip.Success, Is.True);
            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.GreaterThan(0));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.LastCard.HasArrival, Is.False);
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { forge, home }));

            view.ClearPartial();
            PumpUntilIdle(stack, run);

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.GreaterThan(0));
            Assert.That(view.NodeApplyCount, Is.EqualTo(1));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(BothNeighborhoods(run, forge, home)));
            Assert.That(view.AppliedStates[belt].IsExplorable, Is.True);
            Assert.That(view.AppliedStates[choke].IsExplorable, Is.False);
            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(home));
            presenter.Dispose();
        }

        [Test]
        public void Travel_UsesTheSelectedFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(59));
            var home = ContentId.From("home-system");
            var first = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var second),
                Is.True);
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            selection.SelectFleet(second);
            view.ClickNode(ContentId.From("forge-system"));
            view.RightClickNode(ContentId.From("forge-system"));

            Assert.That(selection.SelectedFleetId, Is.EqualTo(second));
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(second), Is.True);
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(first), Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Travel_WithoutAFleet_DoesNotStart()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(60));
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), status, stack);
            presenter.Start();
            view.ClickNode(ContentId.From("forge-system"));

            view.RightClickNode(ContentId.From("forge-system"));

            Assert.That(status.Toasts, Is.Empty);
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Merge_ClearsASelectedFleetThatWasRemoved()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(61));
            var home = ContentId.From("home-system");
            var target = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var source),
                Is.True);
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectFleet(source);

            var merged = stack.FleetService.Merge(run, target, source);

            Assert.That(merged.Success, Is.True);
            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void TimelineReset_ClearsTheSelectedFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(64));
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();

            Assert.That(stack.Manager.RecreateTimeline(1), Is.Not.Null);

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            view.ClickNode(ContentId.From("forge-system"));
            view.RightClickNode(ContentId.From("forge-system"));
            Assert.That(run.Timeline.Fleets.ActiveTravels, Is.Empty);
            presenter.Dispose();
        }

        [Test]
        public void Rewind_RefreshesNodesAndRoutesWhenNoFleetIsSelected()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(102));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, run.Timeline.Fleets[0].Id, forge).Success,
                Is.True);
            Assert.That(view.AppliedStates[home].IsFleetTraveling, Is.True);
            Assert.That(view.LastRoutes, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.InTransit));
            var applies = view.ApplyCount;

            Assert.That(stack.Manager.RecreateTimeline(1), Is.Not.Null);

            Assert.That(view.ApplyCount, Is.GreaterThan(applies));
            Assert.That(view.AppliedStates[home].IsFleetTraveling, Is.False);
            Assert.That(view.LastRoutes, Has.None.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.InTransit));
            presenter.Dispose();
        }

        [Test]
        public void Rewind_ClearsSelectedFleetAndRefreshesNodesAndRoutes()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(103));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectFleet(fleetId);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            Assert.That(view.AppliedStates[home].IsFleetTraveling, Is.True);
            var applies = view.ApplyCount;

            Assert.That(stack.Manager.RecreateTimeline(1), Is.Not.Null);

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            Assert.That(view.ApplyCount, Is.GreaterThan(applies));
            Assert.That(view.AppliedStates[home].IsFleetTraveling, Is.False);
            Assert.That(view.LastRoutes, Has.None.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.InTransit));
            presenter.Dispose();
        }

        [Test]
        public void SelectionChange_UpdatesOnlyTheOldAndNewNode()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(54));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            view.ClickNode(home);

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { home }));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));

            view.ClickNode(forge);

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.CardApplyCount, Is.EqualTo(2));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { home, forge }));
            presenter.Dispose();
        }

        [Test]
        public void Explore_UpdatesRoutesAndTheNeighborhood()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(55));
            var forge = ContentId.From("forge-system");
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;
            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);

            var explored = stack.ExplorationService.Explore(run, ObserverId.Player, forge);

            Assert.That(explored.Success, Is.True);
            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.EqualTo(1));
            Assert.That(view.CardApplyCount, Is.EqualTo(0));
            var expected = new List<ContentId> { forge };
            var neighbors = run.Universe.MapGraph.GetNeighbors(forge);
            for (var i = 0; i < neighbors.Count; i++)
            {
                expected.Add(neighbors[i]);
            }

            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(expected));
            presenter.Dispose();
        }

        [Test]
        public void ForeignMapKnowledge_DoesNotUpdateTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(56));
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            stack.Events.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.From("rival"),
                ContentId.From("forge-system"),
                MapKnowledgeKind.NodeExplored));

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.NodeApplyCount, Is.EqualTo(0));
            Assert.That(view.RouteApplyCount, Is.EqualTo(0));
            Assert.That(view.CardApplyCount, Is.EqualTo(0));
            presenter.Dispose();
        }

        [Test]
        public void Start_PostsNoActiveRunWhenTheManagerIsEmpty()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var status = new OverworldStatus();
            var presenter = CreatePresenter(new FakeStarMapView(), new OverworldSelection(), status, stack);

            presenter.Start();

            Assert.That(status.Text, Is.EqualTo(OverworldStatus.kNoRun));
            presenter.Dispose();
        }

        [Test]
        public void FleetCreated_RepaintsTheEmptySystemAndItsNeighbors()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(90));
            var home = ContentId.From("home-system");
            var belt = ContentId.From("asteroid-belt");
            Assert.That(run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, ContentId.From("forge-system")), Is.True);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();

            Assert.That(
                stack.FleetService.AddBuiltShip(run, home, ContentId.From("starter-screen"), out var created),
                Is.True);

            Assert.That(view.NodeApplyCount, Is.EqualTo(1));
            Assert.That(view.AppliedNodeIds, Does.Contain(home));
            Assert.That(view.AppliedNodeIds, Does.Contain(belt));
            Assert.That(view.AppliedStates[home].HasFleet, Is.True);
            Assert.That(view.AppliedStates[home].Visibility, Is.EqualTo(MapNodeVisibility.Present));
            Assert.That(view.AppliedStates[belt].IsExplorable, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesArrival()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(86));
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectNode(ContentId.From("forge-system"));
            selection.SelectFleet(run.Timeline.Fleets[0].Id);

            Assert.That(view.LastCard.HasArrival, Is.True);
            Assert.That(view.LastCard.ArrivalSeconds, Is.EqualTo(3f).Within(0.001f));

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-travel")), Is.True);
            stack.Effects.Rebuild(run, RunTestFixture.CreateObjectDatabase().ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-travel")));

            Assert.That(view.LastCard.ArrivalSeconds, Is.EqualTo(3f / 1.25f).Within(0.001f));
            presenter.Dispose();
        }

        [Test]
        public void Hover_ShowsPlannedRouteWithoutChangingSelection()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(101));
            var forge = ContentId.From("forge-system");
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            var selectedNode = selection.SelectedNodeId;
            view.ClearPartial();

            view.HoverNode(forge);

            Assert.That(selection.SelectedNodeId, Is.EqualTo(selectedNode));
            Assert.That(view.LastHoverTooltip.IsShown, Is.True);
            Assert.That(view.LastHoverTooltip.Reachable, Is.True);
            Assert.That(view.LastRoutes, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.Planned));
            Assert.That(view.RouteApplyCount, Is.GreaterThan(0));

            view.HoverNode(default);

            Assert.That(view.LastHoverTooltip.IsShown, Is.False);
            Assert.That(view.LastRoutes, Has.None.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.Planned));
            presenter.Dispose();
        }

        [Test]
        public void EnterSystem_ClearsStaleHoverPreview()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(102));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            view.HoverNode(forge);
            Assert.That(view.LastRoutes, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.Planned));

            view.ClickNode(home);

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(view.LastHoverTooltip.IsShown, Is.False);
            Assert.That(view.LastRoutes, Has.None.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.Planned));
            presenter.Dispose();
        }

        private static StarMapPresenter CreatePresenter(
            FakeStarMapView view,
            OverworldSelection selection,
            OverworldTestContext stack)
        {
            return CreatePresenter(view, selection, new OverworldStatus(), stack);
        }

        private static StarMapPresenter CreatePresenter(
            FakeStarMapView view,
            OverworldSelection selection,
            OverworldStatus status,
            OverworldTestContext stack)
        {
            var database = RunTestFixture.CreateObjectDatabase();
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

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected;

            public event Action<ContentId> TravelRequested;

            public event Action<ContentId> NodeHovered;

            public int ApplyCount { get; private set; }

            public int NodeApplyCount { get; private set; }

            public int RouteApplyCount { get; private set; }

            public int CardApplyCount { get; private set; }

            public List<ContentId> AppliedNodeIds { get; } = new();

            public Dictionary<ContentId, MapNodePresentationState> AppliedStates { get; } = new();

            public StarMapCardModel LastCard { get; private set; }

            public StarMapHoverTooltipModel LastHoverTooltip { get; private set; }

            public IReadOnlyList<MapRouteSegment> LastRoutes { get; private set; } = Array.Empty<MapRouteSegment>();

            public IReadOnlyList<FleetMarkerModel> LastFleetMarkers { get; private set; } =
                Array.Empty<FleetMarkerModel>();

            public int FleetMarkerApplyCount { get; private set; }

            public void Apply(StarMapModel model)
            {
                ApplyCount++;
                if (model == null || !model.HasRun)
                {
                    LastRoutes = Array.Empty<MapRouteSegment>();
                    LastFleetMarkers = Array.Empty<FleetMarkerModel>();
                    AppliedNodeIds.Clear();
                    AppliedStates.Clear();
                    return;
                }

                LastRoutes = model.Routes;
                LastFleetMarkers = model.FleetMarkers;
                LastCard = model.Card;
                AppliedNodeIds.Clear();
                AppliedStates.Clear();
                for (var i = 0; i < model.Nodes.Count; i++)
                {
                    AppliedNodeIds.Add(model.Nodes[i].NodeId);
                    AppliedStates[model.Nodes[i].NodeId] = model.Nodes[i].State;
                }
            }

            public void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes)
            {
                NodeApplyCount++;
                AppliedNodeIds.Clear();
                AppliedStates.Clear();
                if (nodes == null)
                {
                    return;
                }

                for (var i = 0; i < nodes.Count; i++)
                {
                    AppliedNodeIds.Add(nodes[i].NodeId);
                    AppliedStates[nodes[i].NodeId] = nodes[i].State;
                }
            }

            public void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes)
            {
                LastRoutes = routes ?? Array.Empty<MapRouteSegment>();
                RouteApplyCount++;
            }

            public void ApplyCard(StarMapCardModel card)
            {
                LastCard = card;
                CardApplyCount++;
            }

            public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
            {
                LastHoverTooltip = tooltip;
            }

            public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
            {
                LastFleetMarkers = markers ?? Array.Empty<FleetMarkerModel>();
                FleetMarkerApplyCount++;
            }

            public void ClearPartial()
            {
                NodeApplyCount = 0;
                RouteApplyCount = 0;
                CardApplyCount = 0;
                FleetMarkerApplyCount = 0;
                AppliedNodeIds.Clear();
                AppliedStates.Clear();
            }

            public void ClickNode(ContentId nodeId)
            {
                NodeSelected?.Invoke(nodeId);
            }

            public void RightClickNode(ContentId nodeId)
            {
                TravelRequested?.Invoke(nodeId);
            }

            public void HoverNode(ContentId nodeId)
            {
                NodeHovered?.Invoke(nodeId);
            }
        }

        private static void PumpUntilIdle(OverworldTestContext stack, RunState run)
        {
            stack.Clock.SetPaused(false);
            var ticks = 0;
            while (run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
            {
                stack.Loop.Pump(1f / 30f);
                ticks++;
            }
        }

        private static List<ContentId> BothNeighborhoods(RunState run, ContentId fromNodeId, ContentId toNodeId)
        {
            List<ContentId> ids = new();
            AddNeighborhood(run, fromNodeId, ids);
            AddNeighborhood(run, toNodeId, ids);
            return ids;
        }

        private static void AddNeighborhood(RunState run, ContentId nodeId, List<ContentId> ids)
        {
            if (!ids.Contains(nodeId))
            {
                ids.Add(nodeId);
            }

            var neighbors = run.Universe.MapGraph.GetNeighbors(nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (!ids.Contains(neighbors[i]))
                {
                    ids.Add(neighbors[i]);
                }
            }
        }
    }
}
