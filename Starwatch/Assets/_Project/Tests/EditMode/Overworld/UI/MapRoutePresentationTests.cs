using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for known-route map presentation.
    /// </summary>
    public sealed class MapRoutePresentationTests
    {
        [Test]
        public void ToNodeCenter_OffsetsByHalfNodeSize()
        {
            var center = MapLayout.ToNodeCenter(
                Vector2.zero,
                scale: 10f,
                originOffset: new Vector2(5f, 7f));

            Assert.That(center.x, Is.EqualTo(5f + (MapLayout.kNodeSize * 0.5f)));
            Assert.That(center.y, Is.EqualTo(7f + (MapLayout.kNodeSize * 0.5f)));
        }

        [Test]
        public void SetRoutes_TracksOneSegmentPerRoute()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.Rebuild(new[]
            {
                new StarMapNodeModel(ContentId.From("a"), MapNodeKind.System, Vector2.zero, default),
                new StarMapNodeModel(ContentId.From("b"), MapNodeKind.System, Vector2.one, default),
            });

            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            });

            Assert.That(map.RouteCount, Is.EqualTo(1));
            Assert.That(surface.Q<VisualElement>("map-route-layer"), Is.Not.Null);
        }

        [Test]
        public void SetRoutes_KeepsTheLayerWhenTheSetIsUnchanged()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var segments = new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            };
            map.SetRoutes(segments);
            var layer = surface.Q<VisualElement>("map-route-layer");

            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            });

            Assert.That(surface.Q<VisualElement>("map-route-layer"), Is.SameAs(layer));
            Assert.That(map.RouteCount, Is.EqualTo(1));
        }

        [Test]
        public void SetRoutes_UpdatesWhenASegmentIsAdded()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            });

            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
                Segment("b", "c", Vector2.one, new Vector2(2f, 0f), MapRouteState.Known),
            });

            Assert.That(map.RouteCount, Is.EqualTo(2));
        }

        [Test]
        public void SetRoutes_UpdatesWhenStateChanges()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            });

            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Planned),
            });

            Assert.That(map.RouteCount, Is.EqualTo(1));
        }

        [Test]
        public void Rebuild_KeepsRoutesAfterTheNodeSetChanges()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.Rebuild(new[]
            {
                new StarMapNodeModel(ContentId.From("a"), MapNodeKind.System, Vector2.zero, default),
            });
            map.SetRoutes(new[]
            {
                Segment("a", "b", Vector2.zero, Vector2.one, MapRouteState.Known),
            });

            map.Rebuild(new[]
            {
                new StarMapNodeModel(ContentId.From("a"), MapNodeKind.System, Vector2.zero, default),
                new StarMapNodeModel(ContentId.From("b"), MapNodeKind.System, Vector2.one, default),
            });

            Assert.That(map.RouteCount, Is.EqualTo(1));
            Assert.That(surface.Q<VisualElement>("map-route-layer"), Is.Not.Null);
        }

        [Test]
        public void Start_KnowsRoutesOutOfExploredHome()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(21));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var belt = ContentId.From("asteroid-belt");
            var choke = ContentId.From("choke-system");

            Assert.That(
                run.Archive.PlayerMapKnowledge.HasKnownRoute(RouteService.MakeRouteKey(home, forge)),
                Is.True);
            Assert.That(
                run.Archive.PlayerMapKnowledge.HasKnownRoute(RouteService.MakeRouteKey(home, belt)),
                Is.True);
            Assert.That(
                run.Archive.PlayerMapKnowledge.HasKnownRoute(RouteService.MakeRouteKey(forge, choke)),
                Is.False);
        }

        [Test]
        public void Explore_RecordsRoutesToUnexploredNeighbors()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(22));
            var forge = ContentId.From("forge-system");
            var choke = ContentId.From("choke-system");
            var key = RouteService.MakeRouteKey(forge, choke);

            Assert.That(run.Archive.PlayerMapKnowledge.HasKnownRoute(key), Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);
            var explored = stack.ExplorationService.Explore(run, ObserverId.Player, forge);

            Assert.That(explored.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(choke), Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasKnownRoute(key), Is.True);
        }

        private static MapRouteSegment Segment(
            string nodeA,
            string nodeB,
            Vector2 positionA,
            Vector2 positionB,
            MapRouteState state)
        {
            return new MapRouteSegment(
                ContentId.From(nodeA),
                ContentId.From(nodeB),
                positionA,
                positionB,
                state);
        }
    }
}
