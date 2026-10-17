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
            view.ClickNode(ContentId.From("forge-system"));
            var beforeTravel = status.Text;

            view.ClickTravel();

            var whileTraveling = status.Text;
            Assert.That(whileTraveling, Is.Not.EqualTo(beforeTravel));
            Assert.That(run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.True);

            stack.Clock.SetPaused(false);
            var ticks = 0;
            while (run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
            {
                stack.Loop.Pump(1f / 30f);
                ticks++;
            }

            Assert.That(run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.False);
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

            stack.Sensor.ScanPlanet(run, ObserverId.Player, ContentId.From("home-prime"));

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
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();
            view.ClickNode(forge);
            view.ClearPartial();

            view.ClickTravel();

            Assert.That(view.ApplyCount, Is.EqualTo(1));
            Assert.That(view.RouteApplyCount, Is.EqualTo(0));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { home, forge }));
            Assert.That(run.Timeline.HasTravelingFleetAt(home), Is.True);
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
            var view = new FakeStarMapView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();

            var outbound = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, forge);
            Assert.That(outbound.Success, Is.True);
            PumpUntilIdle(stack, run);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(home), Is.True);

            view.ClickNode(choke);
            Assert.That(view.LastCard.CanTravel, Is.True);
            view.ClearPartial();
            var applies = view.ApplyCount;

            var returnTrip = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, home);
            Assert.That(returnTrip.Success, Is.True);
            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.EqualTo(0));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.LastCard.CanTravel, Is.False);
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(new[] { forge, home }));

            view.ClearPartial();
            PumpUntilIdle(stack, run);

            Assert.That(view.ApplyCount, Is.EqualTo(applies));
            Assert.That(view.RouteApplyCount, Is.EqualTo(0));
            Assert.That(view.NodeApplyCount, Is.EqualTo(1));
            Assert.That(view.CardApplyCount, Is.EqualTo(1));
            Assert.That(view.AppliedNodeIds, Is.EquivalentTo(BothNeighborhoods(run, forge, home)));
            Assert.That(view.AppliedStates[belt].IsExplorable, Is.True);
            Assert.That(view.AppliedStates[choke].IsExplorable, Is.False);
            Assert.That(run.Timeline.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(home));
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
            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);

            var explored = stack.Exploration.Explore(run, ObserverId.Player, forge);

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

        private sealed class FakeStarMapView : IStarMapView
        {
            public event Action<ContentId> NodeSelected;

            public event Action TravelRequested;

            public int ApplyCount { get; private set; }

            public int NodeApplyCount { get; private set; }

            public int RouteApplyCount { get; private set; }

            public int CardApplyCount { get; private set; }

            public List<ContentId> AppliedNodeIds { get; } = new();

            public Dictionary<ContentId, MapNodePresentationState> AppliedStates { get; } = new();

            public StarMapCardModel LastCard { get; private set; }

            public void Apply(StarMapModel model)
            {
                ApplyCount++;
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
                _ = routes;
                RouteApplyCount++;
            }

            public void ApplyCard(StarMapCardModel card)
            {
                LastCard = card;
                CardApplyCount++;
            }

            public void ClearPartial()
            {
                NodeApplyCount = 0;
                RouteApplyCount = 0;
                CardApplyCount = 0;
                AppliedNodeIds.Clear();
                AppliedStates.Clear();
            }

            public void ClickNode(ContentId nodeId)
            {
                NodeSelected?.Invoke(nodeId);
            }

            public void ClickTravel()
            {
                TravelRequested?.Invoke();
            }
        }

        private static void PumpUntilIdle(OverworldTestContext stack, RunState run)
        {
            stack.Clock.SetPaused(false);
            var ticks = 0;
            while (run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
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
