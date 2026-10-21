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
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(41));
            var query = CreateQuery(stack);

            var routes = query.KnownRoutes(run);

            Assert.That(routes, Has.Count.EqualTo(2));
            Assert.That(routes[0].State, Is.EqualTo(MapRouteState.Known));
        }

        [Test]
        public void NodeState_KeepsHiddenNeighborExplorableWhileFleetTravels()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(42));
            var query = CreateQuery(stack);
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
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
        public void Describe_OffersTravelIntoHiddenNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(43));
            var query = CreateQuery(stack);
            var forge = ContentId.From("forge-system");
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
            Assert.That(card.ArrivalSeconds, Is.EqualTo(3f).Within(0.001f));
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
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(98));
            var query = CreateQuery(stack);
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
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
        public void TravelPreview_MatchesTripSecondsWhenReachable()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(99));
            var query = CreateQuery(stack);
            var fleetId = run.Timeline.Fleets[0].Id;
            var forge = ContentId.From("forge-system");

            var preview = query.TravelPreview(run, fleetId, forge);

            Assert.That(preview.Reachable, Is.True);
            Assert.That(preview.ToNodeId, Is.EqualTo(forge));
            Assert.That(preview.Seconds, Is.EqualTo(3f).Within(0.001f));
            Assert.That(query.TravelPreview(run, fleetId, ContentId.From("choke-system")).Reachable, Is.False);
        }

        [Test]
        public void DescribeRoutes_MarksInTransitAndPlanned()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(100));
            var query = CreateQuery(stack);
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
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
        public void CanEnter_RequiresTheLocationToBeExplored()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(44));
            var query = CreateQuery(stack);
            var belt = ContentId.From("asteroid-belt");

            Assert.That(query.CanEnter(run, belt), Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.ExplorationService.Explore(run, ObserverId.Player, belt);

            Assert.That(query.CanEnter(run, belt), Is.True);
        }

        [Test]
        public void Describe_KeepsTheCardWhenTheSystemIsOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(45));
            var query = CreateQuery(stack);
            var home = ContentId.From("home-system");
            var closed = new OverworldSelection();
            closed.SelectNode(home);
            var open = new OverworldSelection();
            open.EnterSystem(home);

            var onMap = query.Describe(run, closed);
            var inSystem = query.Describe(run, open);

            Assert.That(onMap.HasSelection, Is.True);
            Assert.That(inSystem.HasSelection, Is.True);
            Assert.That(onMap.Card.IsExplored, Is.True);
            Assert.That(inSystem.Card.IsExplored, Is.True);
            Assert.That(onMap.Title, Is.EqualTo(inSystem.Title));
            Assert.That(onMap.Card.BodyCount, Is.EqualTo(inSystem.Card.BodyCount));
        }

        private static StarMapQuery CreateQuery(OverworldTestContext stack)
        {
            var database = RunTestFixture.CreateObjectDatabase();
            return new StarMapQuery(
                stack.VisibilityService,
                stack.TravelService,
                new FleetListQuery(database, stack.Effects, stack.Clock),
                database);
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
