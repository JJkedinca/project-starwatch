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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 46);
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            view.ClickNode(RunTestFixture.HomeSystem(run));

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);

            selection.SelectPlanet(RunTestFixture.HomePlanet(run));
            view.ClickNode(RunTestFixture.HomeSystem(run));

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);

            view.ClickNode(RunTestFixture.FirstNeighbor(run));

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(RunTestFixture.FirstNeighbor(run)));
            presenter.Dispose();
        }

        [Test]
        public void Travel_PostsTravelingThenArrived()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 47);
            var selection = new OverworldSelection();
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            view.ClickNode(RunTestFixture.FirstNeighbor(run));
            var beforeTravel = status.Text;

            view.RightClickNode(RunTestFixture.FirstNeighbor(run));

            var whileTraveling = status.Text;
            Assert.That(whileTraveling, Is.Not.EqualTo(beforeTravel));
            var fleetId = run.Timeline.Fleets[0].Id;
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(fleetId), Is.True);

            PumpUntilIdle(stack, run, fleetId);

            Assert.That(run.Timeline.Fleets.IsFleetTraveling(fleetId), Is.False);
            Assert.That(status.Text, Is.Not.EqualTo(whileTraveling));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(RunTestFixture.FirstNeighbor(run)), Is.True);
            presenter.Dispose();
        }

        [Test]
        public void PlanetScan_DoesNotRebuildTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 48);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            stack.SensorService.ScanPlanet(run, ObserverId.Player, RunTestFixture.HomePlanet(run));

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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 53);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            view.ClickNode(forge);
            view.ClearPartial();

            view.RightClickNode(RunTestFixture.FirstNeighbor(run));

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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 103);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectFleet(fleetId);

            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            Assert.That(view.LastFleetMarkers, Has.Some.Matches<FleetMarkerModel>(
                marker => marker.Kind == FleetMarkerKind.Transit));

            PumpUntilIdle(stack, run, fleetId);

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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 57);
            var home = RunTestFixture.HomeSystem(run);
            var homeNeighbors = run.Universe.MapGraph.GetNeighbors(home);
            Assert.That(homeNeighbors.Count, Is.GreaterThan(1));
            var belt = homeNeighbors[1];
            ContentId forge = default;
            ContentId forgeBranch = default;
            for (var i = 0; i < homeNeighbors.Count; i++)
            {
                var candidate = homeNeighbors[i];
                var candidateNeighbors = run.Universe.MapGraph.GetNeighbors(candidate);
                for (var j = 0; j < candidateNeighbors.Count; j++)
                {
                    if (candidateNeighbors[j] != home)
                    {
                        forge = candidate;
                        forgeBranch = candidateNeighbors[j];
                        break;
                    }
                }

                if (!forge.IsEmpty)
                {
                    break;
                }
            }

            Assert.That(forge.IsEmpty, Is.False);
            Assert.That(forgeBranch.IsEmpty, Is.False);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var outbound = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge);
            Assert.That(outbound.Success, Is.True);
            PumpUntilIdle(stack, run, fleetId);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(home), Is.True);

            selection.SelectFleet(fleetId);
            view.ClickNode(forgeBranch);
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
            PumpUntilIdle(stack, run, fleetId);

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.GreaterThan(0));
            Assert.That(view.NodeApplyCount, Is.EqualTo(1));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(BothNeighborhoods(run, forge, home)));
            Assert.That(view.AppliedStates[belt].IsExplorable, Is.True);
            Assert.That(view.AppliedStates[forgeBranch].IsExplorable, Is.False);
            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(home));
            presenter.Dispose();
        }

        [Test]
        public void Travel_UsesTheSelectedFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 59);
            var home = RunTestFixture.HomeSystem(run);
            var first = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var second),
                Is.True);
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            selection.SelectFleet(second);
            view.ClickNode(RunTestFixture.FirstNeighbor(run));
            view.RightClickNode(RunTestFixture.FirstNeighbor(run));

            Assert.That(selection.SelectedFleetId, Is.EqualTo(second));
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(second), Is.True);
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(first), Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Travel_WithoutAFleet_DoesNotStart()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 60);
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), status, stack);
            presenter.Start();
            view.ClickNode(RunTestFixture.FirstNeighbor(run));

            view.RightClickNode(RunTestFixture.FirstNeighbor(run));

            Assert.That(status.Toasts, Is.Empty);
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Merge_ClearsASelectedFleetThatWasRemoved()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 61);
            var home = RunTestFixture.HomeSystem(run);
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 64);
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            var status = new OverworldStatus();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, status, stack);
            presenter.Start();

            Assert.That(stack.Manager.RecreateTimeline(1), Is.Not.Null);

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            view.ClickNode(RunTestFixture.FirstNeighbor(run));
            view.RightClickNode(RunTestFixture.FirstNeighbor(run));
            Assert.That(run.Timeline.Fleets.ActiveTravels, Is.Empty);
            presenter.Dispose();
        }

        [Test]
        public void Rewind_RefreshesNodesAndRoutesWhenNoFleetIsSelected()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 102);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 103);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
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
        public void SelectionChange_FullApplyOnEnterAndLeaveSystem()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 54);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            view.ClickNode(home);

            // Enter/leave system repaints the full map so galaxy labels restore after Back.
            Assert.That(view.ApplyCount, Is.EqualTo(applies + 1));
            Assert.That(view.AppliedNodeIds, Does.Contain(home));

            view.ClickNode(forge);

            Assert.That(view.ApplyCount, Is.EqualTo(applies + 2));
            Assert.That(view.AppliedNodeIds, Does.Contain(forge));
            presenter.Dispose();
        }

        [Test]
        public void Explore_UpdatesRoutesAndTheNeighborhood()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 55);
            var forge = RunTestFixture.FirstNeighbor(run);
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 56);
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            var applies = view.ApplyCount;

            stack.Events.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.From("rival"),
                RunTestFixture.FirstNeighbor(run),
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 90);
            var home = RunTestFixture.HomeSystem(run);
            var homeNeighbors = run.Universe.MapGraph.GetNeighbors(RunTestFixture.HomeSystem(run));
            Assert.That(homeNeighbors.Count, Is.GreaterThan(1));
            var belt = homeNeighbors[1];
            Assert.That(run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, RunTestFixture.FirstNeighbor(run)), Is.True);
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 86);
            var neighbor = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            selection.SelectNode(neighbor);
            selection.SelectFleet(fleetId);
            Assert.That(
                stack.TravelService.TryGetTripSeconds(run, ObserverId.Player, fleetId, neighbor, out var tripSeconds),
                Is.True);

            Assert.That(view.LastCard.HasArrival, Is.True);
            Assert.That(view.LastCard.ArrivalSeconds, Is.EqualTo(tripSeconds).Within(0.001f));

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-travel")), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-travel")));

            Assert.That(view.LastCard.ArrivalSeconds, Is.EqualTo(tripSeconds / 1.25f).Within(0.001f));
            presenter.Dispose();
        }

        [Test]
        public void Hover_ShowsPlannedRouteWithoutChangingSelection()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 101);
            var forge = RunTestFixture.FirstNeighbor(run);
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
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 102);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
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

        private static void PumpUntilIdle(OverworldTestContext stack, RunState run, EntityId fleetId)
        {
            stack.Clock.SetPaused(false);
            var timeout = 120f;
            var travels = run.Timeline.Fleets.ActiveTravels;
            if (travels.Count > 0)
            {
                timeout = (float)(travels[0].ArriveTime.Seconds - stack.Clock.CurrentTime.Seconds) + 5f;
            }

            RunTestFixture.PumpUntilFleetIdle(stack, run, fleetId, timeout);
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
