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
    /// Star map read model: routes, node cards, and whether a system can be opened.
    /// </summary>
    public sealed class StarMapQueryTests
    {
        [Test]
        public void KnownRoutes_IncludesRoutesOutOfExploredHome()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(41));
            var query = new StarMapQuery(stack.Visibility, stack.Travel);

            var routes = query.KnownRoutes(run);

            Assert.That(routes, Has.Count.EqualTo(2));
        }

        [Test]
        public void NodeState_KeepsHiddenNeighborExplorableWhileFleetTravels()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(42));
            var query = new StarMapQuery(stack.Visibility, stack.Travel);
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var fleetId = run.Timeline.Fleets[0].Id;

            var started = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, forge);

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
            Assert.That(card.CanTravel, Is.False);
        }

        [Test]
        public void Describe_OffersTravelIntoHiddenNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(43));
            var query = new StarMapQuery(stack.Visibility, stack.Travel);
            var forge = ContentId.From("forge-system");
            var selection = new OverworldSelection();
            selection.SelectNode(forge);

            var card = query.Describe(run, selection);

            Assert.That(card.HasSelection, Is.True);
            Assert.That(card.CanTravel, Is.True);
            Assert.That(query.TryFindFleetForDestination(run, forge, out var fleetId), Is.True);
            Assert.That(fleetId, Is.EqualTo(run.Timeline.Fleets[0].Id));
        }

        [Test]
        public void CanEnter_RequiresTheLocationToBeExplored()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(44));
            var query = new StarMapQuery(stack.Visibility, stack.Travel);
            var belt = ContentId.From("asteroid-belt");

            Assert.That(query.CanEnter(run, belt), Is.False);

            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.Exploration.Explore(run, ObserverId.Player, belt);

            Assert.That(query.CanEnter(run, belt), Is.True);
        }

        [Test]
        public void Describe_OmitsTheOpenHintWhileTheSystemIsOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(45));
            var query = new StarMapQuery(stack.Visibility, stack.Travel);
            var home = ContentId.From("home-system");
            var closed = new OverworldSelection();
            closed.SelectNode(home);
            var open = new OverworldSelection();
            open.EnterSystem(home);

            var onMap = query.Describe(run, closed);
            var inSystem = query.Describe(run, open);

            Assert.That(onMap.HasSelection, Is.True);
            Assert.That(inSystem.HasSelection, Is.True);
            Assert.That(onMap.Title, Is.EqualTo(home.Value));
            Assert.That(inSystem.Title, Is.EqualTo(home.Value));
            Assert.That(onMap.Summary, Does.StartWith(inSystem.Summary));
            Assert.That(onMap.Summary.Length, Is.GreaterThan(inSystem.Summary.Length));
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
