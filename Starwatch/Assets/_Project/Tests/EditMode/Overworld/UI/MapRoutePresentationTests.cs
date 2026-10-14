using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
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
        public void SetRoutes_CreatesOneElementPerSegment()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.Rebuild(new[]
            {
                new MapNode(ContentId.From("a"), MapNodeKind.System, Vector2.zero, null),
                new MapNode(ContentId.From("b"), MapNodeKind.System, Vector2.one, null),
            });

            map.SetRoutes(new[]
            {
                new MapRouteSegment(Vector2.zero, Vector2.one),
            });

            Assert.That(map.RouteCount, Is.EqualTo(1));
        }

        [Test]
        public void SetRoutes_BoundsMatchNodeElementCenters()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.Rebuild(new[]
            {
                new MapNode(ContentId.From("a"), MapNodeKind.System, Vector2.zero, null),
                new MapNode(ContentId.From("b"), MapNodeKind.System, Vector2.one, null),
            });
            map.SetRoutes(new[]
            {
                new MapRouteSegment(Vector2.zero, Vector2.one),
            });

            var nodeA = surface.Q<Button>("node-a");
            var nodeB = surface.Q<Button>("node-b");
            var route = surface.Q<VisualElement>("map-route");
            Assert.That(nodeA, Is.Not.Null);
            Assert.That(nodeB, Is.Not.Null);
            Assert.That(route, Is.Not.Null);
            Assert.That(nodeA.style.width.value.value, Is.EqualTo(MapLayout.kNodeSize));
            Assert.That(nodeA.style.height.value.value, Is.EqualTo(MapLayout.kNodeSize));

            var centerA = ElementCenter(nodeA);
            var centerB = ElementCenter(nodeB);
            var min = Vector2.Min(centerA, centerB);
            var max = Vector2.Max(centerA, centerB);
            var pad = MapLayout.kRoutePad;

            Assert.That(route.style.left.value.value, Is.EqualTo(min.x - pad).Within(0.01f));
            Assert.That(route.style.top.value.value, Is.EqualTo(min.y - pad).Within(0.01f));
            Assert.That(
                route.style.width.value.value,
                Is.EqualTo(Mathf.Max(1f, max.x - min.x) + (pad * 2f)).Within(0.01f));
            Assert.That(
                route.style.height.value.value,
                Is.EqualTo(Mathf.Max(1f, max.y - min.y) + (pad * 2f)).Within(0.01f));
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

            var explored = stack.Exploration.Explore(run, ObserverId.Player, forge);

            Assert.That(explored.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(choke), Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasKnownRoute(key), Is.True);
        }

        private static Vector2 ElementCenter(VisualElement element)
        {
            return new Vector2(
                element.style.left.value.value + (element.style.width.value.value * 0.5f),
                element.style.top.value.value + (element.style.height.value.value * 0.5f));
        }
    }
}
