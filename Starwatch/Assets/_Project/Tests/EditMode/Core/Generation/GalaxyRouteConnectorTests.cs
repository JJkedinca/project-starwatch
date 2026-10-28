using System.Collections.Generic;
using NUnit.Framework;
using ProjectStarwatch.Core.Generation;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.Generation
{
    /// <summary>
    /// EditMode coverage for shape-aware route connection.
    /// </summary>
    public sealed class GalaxyRouteConnectorTests
    {
        [Test]
        public void TryConnect_DropsForbiddenRouteCostEdges()
        {
            var shape = ScriptableObject.CreateInstance<StubShape>();
            shape.Forbidden = (a, b) => Nearly(a, new Vector2(0f, 0f)) && Nearly(b, new Vector2(1f, 0f));

            var layout = Nodes(
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0.8f),
                new Vector2(0.5f, 0.2f));
            var ids = Ids(layout.Count);
            var map = Map(layout);

            Assert.That(
                GalaxyRouteConnector.TryConnect(ids, map, layout, shape, 40f, out var routes),
                Is.True);

            Assert.That(HasRoute(routes, ids[0], ids[1]), Is.False);

            Object.DestroyImmediate(shape);
        }

        [Test]
        public void TryConnect_RespectsRouteGroupCap()
        {
            var shape = ScriptableObject.CreateInstance<StubShape>();
            shape.GroupCap = 1;
            shape.InGroup = (a, b) =>
                (Nearly(a, new Vector2(0f, 0f)) && Nearly(b, new Vector2(1f, 0f)))
                || (Nearly(a, new Vector2(0f, 1f)) && Nearly(b, new Vector2(1f, 1f)));

            var layout = Nodes(
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f));
            var ids = Ids(layout.Count);
            var map = Map(layout);

            Assert.That(
                GalaxyRouteConnector.TryConnect(ids, map, layout, shape, 40f, out var routes),
                Is.True);

            var groupKept = 0;
            if (HasRoute(routes, ids[0], ids[1]))
            {
                groupKept++;
            }

            if (HasRoute(routes, ids[2], ids[3]))
            {
                groupKept++;
            }

            Assert.That(groupKept, Is.LessThanOrEqualTo(1));

            Object.DestroyImmediate(shape);
        }

        [Test]
        public void TryConnect_TwoNodes_ConnectsWithSingleRoute()
        {
            var shape = ScriptableObject.CreateInstance<StubShape>();
            var layout = Nodes(new Vector2(0f, 0f), new Vector2(1f, 0f));
            var ids = Ids(layout.Count);
            var map = Map(layout);

            Assert.That(
                GalaxyRouteConnector.TryConnect(ids, map, layout, shape, 40f, out var routes),
                Is.True);
            Assert.That(routes.Count, Is.EqualTo(1));
            Assert.That(HasRoute(routes, ids[0], ids[1]), Is.True);

            Object.DestroyImmediate(shape);
        }

        [Test]
        public void TryConnect_NearbyCluster_PrefersLocalDelaunayLinks()
        {
            var shape = ScriptableObject.CreateInstance<StubShape>();
            var layout = Nodes(
                new Vector2(0.2f, 0.2f),
                new Vector2(0.3f, 0.25f),
                new Vector2(0.25f, 0.35f),
                new Vector2(0.8f, 0.8f),
                new Vector2(0.85f, 0.75f),
                new Vector2(0.75f, 0.85f));
            var ids = Ids(layout.Count);
            var map = Map(layout);

            Assert.That(
                GalaxyRouteConnector.TryConnect(ids, map, layout, shape, 40f, out var routes),
                Is.True);

            // Local triangle neighbors should link; a far chord is not required.
            Assert.That(HasRoute(routes, ids[0], ids[1]) || HasRoute(routes, ids[0], ids[2]), Is.True);
            Assert.That(routes.Count, Is.GreaterThanOrEqualTo(layout.Count - 1));

            Object.DestroyImmediate(shape);
        }

        private static List<GeneratedGalaxyNode> Nodes(params Vector2[] positions)
        {
            var nodes = new List<GeneratedGalaxyNode>(positions.Length);
            for (var i = 0; i < positions.Length; i++)
            {
                nodes.Add(new GeneratedGalaxyNode(positions[i], new NodeShapeTags(0, 0, 0f, isCore: false)));
            }

            return nodes;
        }

        private static ContentId[] Ids(int count)
        {
            var ids = new ContentId[count];
            for (var i = 0; i < count; i++)
            {
                ids[i] = ContentId.From($"n-{i}");
            }

            return ids;
        }

        private static Vector2[] Map(IReadOnlyList<GeneratedGalaxyNode> layout)
        {
            var map = new Vector2[layout.Count];
            for (var i = 0; i < layout.Count; i++)
            {
                map[i] = layout[i].Position * 10f;
            }

            return map;
        }

        private static bool HasRoute(
            IReadOnlyList<ProjectStarwatch.Core.Data.MapRouteDefinition> routes,
            ContentId a,
            ContentId b)
        {
            for (var i = 0; i < routes.Count; i++)
            {
                var route = routes[i];
                if ((route.NodeAId == a && route.NodeBId == b) || (route.NodeAId == b && route.NodeBId == a))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Nearly(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude < 0.0001f;
        }

        private sealed class StubShape : GalaxyShapeSO
        {
            public System.Func<Vector2, Vector2, bool> Forbidden { get; set; }

            public System.Func<Vector2, Vector2, bool> InGroup { get; set; }

            public int GroupCap { get; set; } = 1;

            public override int SectorCount => 1;

            public override float Density(Vector2 point) => 1f;

            public override NodeShapeTags Describe(Vector2 point) => new(0, 0, 0f, isCore: false);

            public override float RouteCost(Vector2 a, Vector2 b)
            {
                if (Forbidden != null && (Forbidden(a, b) || Forbidden(b, a)))
                {
                    return float.PositiveInfinity;
                }

                return 1f;
            }

            public override bool TryGetRouteGroup(Vector2 a, Vector2 b, out int groupId, out int maxRoutes)
            {
                groupId = 1;
                maxRoutes = GroupCap;
                if (InGroup == null)
                {
                    return false;
                }

                return InGroup(a, b) || InGroup(b, a);
            }
        }
    }
}
