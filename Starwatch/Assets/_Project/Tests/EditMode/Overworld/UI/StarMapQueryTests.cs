using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Star map read model: routes, node cards, travel preview, and whether a system can be opened.
    /// </summary>
    public sealed class StarMapQueryTests
    {
        [Test]
        public void KnownRoutes_IncludesRoutesOutOfExploredHome()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 41);
            var query = CreateQuery(stack);

            var home = RunTestFixture.HomeSystem(run);
            var routes = query.KnownRoutes(run);
            var homeNeighborCount = run.Universe.MapGraph.GetNeighbors(home).Count;

            Assert.That(homeNeighborCount, Is.GreaterThan(0));
            Assert.That(routes, Has.Count.EqualTo(homeNeighborCount));
            Assert.That(routes, Has.All.Matches<MapRouteSegment>(segment => segment.State == MapRouteState.Known));
        }

        [Test]
        public void NodeState_KeepsHiddenNeighborExplorableWhileFleetTravels()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 42);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;

            var started = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge);

            Assert.That(started.Success, Is.True);
            var forgeState = query.NodeState(run, RequireNode(run, forge), default(ContentId));
            var homeState = query.NodeState(run, RequireNode(run, home), default(ContentId));
            var selection = new OverworldSelection();
            selection.SelectNode(forge);
            var card = query.Describe(run, selection);

            Assert.That(forgeState.Visibility, Is.EqualTo(MapNodeVisibility.Hidden));
            Assert.That(forgeState.IsExplorable, Is.True);
            Assert.That(forgeState.RevealedLabel, Is.Empty);
            Assert.That(forgeState.HasFleet, Is.False);
            Assert.That(homeState.HasFleet, Is.True);
            Assert.That(homeState.IsFleetTraveling, Is.True);
            Assert.That(card.HasSelection, Is.True);
            Assert.That(card.HasArrival, Is.False);
            Assert.That(card.Card.FleetRelation, Is.EqualTo(StarMapFleetRelation.NoneSelected));
        }

        [Test]
        public void NodeState_KeepsKnownHiddenNeighborsAfterFleetLeavesHome()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 44);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var homeNeighbors = run.Universe.MapGraph.GetNeighbors(RunTestFixture.HomeSystem(run));
            Assert.That(homeNeighbors.Count, Is.GreaterThan(1));
            var belt = homeNeighbors[1];
            var fleetId = run.Timeline.Fleets[0].Id;

            Assert.That(query.NodeState(run, RequireNode(run, belt), default).IsExplorable, Is.True);

            var tripSeconds = TripSecondsToNeighbor(stack, run, fleetId, forge);
            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success,
                Is.True);
            RunTestFixture.PumpUntilFleetIdle(stack, run, fleetId, tripSeconds + 5f);

            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(forge));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Timeline.Fleets.HasFleetAt(home), Is.False);

            var beltAfter = query.NodeState(run, RequireNode(run, belt), default);
            Assert.That(beltAfter.Visibility, Is.EqualTo(MapNodeVisibility.Hidden));
            Assert.That(beltAfter.IsExplorable, Is.True);
        }

        [Test]
        public void Describe_OffersTravelIntoHiddenNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 43);
            var query = CreateQuery(stack);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectNode(forge);
            selection.SelectFleet(fleetId);

            var card = query.Describe(run, selection);
            var unselected = new OverworldSelection();
            unselected.SelectNode(forge);

            Assert.That(card.HasSelection, Is.True);
            Assert.That(card.Card.FleetRelation, Is.EqualTo(StarMapFleetRelation.Reachable));
            Assert.That(card.HasArrival, Is.True);
            Assert.That(
                card.ArrivalSeconds,
                Is.EqualTo(TripSecondsToNeighbor(stack, run, fleetId, forge)).Within(0.001f));
            Assert.That(card.Card.ShowTravelHint, Is.True);
            Assert.That(card.Fleets, Has.Count.EqualTo(1));
            Assert.That(card.Fleets[0].FleetId, Is.EqualTo(fleetId));
            Assert.That(card.Fleets[0].IsSelected, Is.True);
            Assert.That(
                query.Describe(run, unselected).Card.FleetRelation,
                Is.EqualTo(StarMapFleetRelation.NoneSelected));
        }

        [Test]
        public void DescribeCard_MarksAtTargetAndInTransit()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 98);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectNode(home);
            selection.SelectFleet(fleetId);

            Assert.That(query.DescribeCard(run, selection).FleetRelation, Is.EqualTo(StarMapFleetRelation.AtTarget));

            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            selection.SelectNode(forge);

            Assert.That(query.DescribeCard(run, selection).FleetRelation, Is.EqualTo(StarMapFleetRelation.InTransit));
        }

        [Test]
        public void DescribeCard_HidesWhileAStarSystemIsOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 96);
            var query = CreateQuery(stack);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));

            var card = query.DescribeCard(run, selection);

            Assert.That(card.IsShown, Is.False);
            Assert.That(card.HasSelection, Is.False);
        }

        [Test]
        public void DescribeCard_UsesCompositionTitleForFleetName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 97);
            var query = CreateQuery(stack);
            var database = stack.ObjectDatabase;
            var home = RunTestFixture.HomeSystem(run);
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("line-ship"), 4, home, out var mixedId),
                Is.True);
            Assert.That(
                run.Timeline.Fleets.TryAddShips(mixedId, ContentId.From("starter-screen"), 2),
                Is.True);
            Assert.That(run.Timeline.Fleets.TryGetFleet(mixedId, out var fleet), Is.True);
            var selection = new OverworldSelection();
            selection.SelectNode(home);
            selection.SelectFleet(mixedId);

            var card = query.DescribeCard(run, selection);
            var expected = OverworldNames.FleetTitle(database, fleet);

            Assert.That(card.FleetName, Is.EqualTo(expected));
            Assert.That(card.FleetName, Does.Contain("4"));
            Assert.That(card.FleetName, Does.Contain("2"));
            Assert.That(database.ShipCatalog.TryGet(ContentId.From("line-ship"), out var line), Is.True);
            Assert.That(card.FleetName, Is.Not.EqualTo(line.DisplayName));
        }

        [Test]
        public void TravelPreview_MatchesTripSecondsWhenReachable()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 99);
            var query = CreateQuery(stack);
            var fleetId = run.Timeline.Fleets[0].Id;
            var forge = RunTestFixture.FirstNeighbor(run);

            var preview = query.TravelPreview(run, fleetId, forge);

            Assert.That(preview.Reachable, Is.True);
            Assert.That(preview.ToNodeId, Is.EqualTo(forge));
            Assert.That(
                preview.Seconds,
                Is.EqualTo(TripSecondsToNeighbor(stack, run, fleetId, forge)).Within(0.001f));
            Assert.That(
                query.TravelPreview(run, fleetId, ContentId.From("missing-system")).Reachable,
                Is.False);
        }

        [Test]
        public void DescribeRoutes_MarksInTransitAndPlanned()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 100);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectFleet(fleetId);

            var planned = query.DescribeRoutes(run, selection, forge);
            Assert.That(planned, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.Planned
                    && RouteService.MakeRouteKey(segment.NodeAId, segment.NodeBId)
                        == RouteService.MakeRouteKey(home, forge)));

            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            var traveling = query.DescribeRoutes(run, selection, default);
            Assert.That(traveling, Has.Some.Matches<MapRouteSegment>(
                segment => segment.State == MapRouteState.InTransit
                    && RouteService.MakeRouteKey(segment.NodeAId, segment.NodeBId)
                        == RouteService.MakeRouteKey(home, forge)));
        }

        [Test]
        public void DescribeFleetMarkers_ParkedShowsCountAndSelection()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 101);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var first = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var second),
                Is.True);
            var selection = new OverworldSelection();
            selection.SelectFleet(second);

            var markers = query.DescribeFleetMarkers(run, selection);

            Assert.That(markers, Has.Count.EqualTo(1));
            Assert.That(markers[0].Kind, Is.EqualTo(FleetMarkerKind.Parked));
            Assert.That(markers[0].NodeId, Is.EqualTo(home));
            Assert.That(markers[0].Count, Is.EqualTo(2));
            Assert.That(markers[0].IsSelected, Is.True);
            Assert.That(markers[0].IsTransit, Is.False);
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void DescribeFleetMarkers_TransitReplacesParkedAtOrigin()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 102);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var forge = RunTestFixture.FirstNeighbor(run);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectFleet(fleetId);

            var idle = query.DescribeFleetMarkers(run, selection);
            Assert.That(idle, Has.Count.EqualTo(1));
            Assert.That(idle[0].Kind, Is.EqualTo(FleetMarkerKind.Parked));

            var tripSeconds = TripSecondsToNeighbor(stack, run, fleetId, forge);
            Assert.That(stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success, Is.True);
            var traveling = query.DescribeFleetMarkers(run, selection);

            Assert.That(traveling, Has.Count.EqualTo(1));
            Assert.That(traveling[0].Kind, Is.EqualTo(FleetMarkerKind.Transit));
            Assert.That(traveling[0].FleetId, Is.EqualTo(fleetId));
            Assert.That(traveling[0].NodeId, Is.EqualTo(home));
            Assert.That(traveling[0].DestinationNodeId, Is.EqualTo(forge));
            Assert.That(traveling[0].IsSelected, Is.True);
            Assert.That(traveling[0].RemainingSeconds, Is.EqualTo(tripSeconds).Within(0.001f));
            Assert.That(traveling[0].TotalSeconds, Is.EqualTo(tripSeconds).Within(0.001f));
        }

        [Test]
        public void CanEnter_RequiresTheLocationToBeExplored()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 44);
            var query = CreateQuery(stack);
            var homeNeighbors = run.Universe.MapGraph.GetNeighbors(RunTestFixture.HomeSystem(run));
            Assert.That(homeNeighbors.Count, Is.GreaterThan(1));
            var belt = homeNeighbors[1];

            Assert.That(query.CanEnter(run, belt), Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.ExplorationService.Explore(run, ObserverId.Player, belt);

            Assert.That(query.CanEnter(run, belt), Is.True);
        }

        [Test]
        public void Describe_HidesTheCardWhenTheSystemOrAnOverlayIsOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 45);
            var query = CreateQuery(stack);
            var home = RunTestFixture.HomeSystem(run);
            var onMap = new OverworldSelection();
            onMap.SelectNode(home);
            var inSystem = new OverworldSelection();
            inSystem.EnterSystem(home);
            var research = new OverworldSelection();
            research.SelectNode(home);
            research.OpenResearch();

            Assert.That(query.Describe(run, onMap).Card.IsShown, Is.True);
            Assert.That(query.Describe(run, onMap).Card.IsExplored, Is.True);
            Assert.That(query.Describe(run, inSystem).Card.IsShown, Is.False);
            Assert.That(query.Describe(run, research).Card.IsShown, Is.False);
        }

        private static StarMapQuery CreateQuery(OverworldTestContext stack)
        {
            var database = stack.ObjectDatabase;
            return new StarMapQuery(
                stack.VisibilityService,
                stack.TravelService,
                new FleetListQuery(database, stack.Effects, stack.Clock),
                database,
                stack.Clock);
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }

        private static float TripSecondsToNeighbor(
            OverworldTestContext stack,
            RunState run,
            EntityId fleetId,
            ContentId neighborId)
        {
            Assert.That(
                stack.TravelService.TryGetTripSeconds(run, ObserverId.Player, fleetId, neighborId, out var seconds),
                Is.True);
            return seconds;
        }
    }
}
