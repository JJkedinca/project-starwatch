using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Universe;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for strict MapGraph construction rules.
    /// </summary>
    public sealed class MapGraphTests
    {
        [Test]
        public void TryCreate_RejectsNegativeDuration()
        {
            var nodes = new[]
            {
                new MapNode(ContentId.From("a"), MapNodeKind.System, Vector2.zero, null),
                new MapNode(ContentId.From("b"), MapNodeKind.System, Vector2.one, null),
            };

            Assert.That(
                () => new MapRoute(ContentId.From("a"), ContentId.From("b"), -1f),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());

            Assert.That(
                MapGraph.TryCreate(
                    nodes,
                    new[] { new MapRoute(ContentId.From("a"), ContentId.From("b"), 1f) },
                    out _,
                    out _),
                Is.True);
        }

        [Test]
        public void TryCreate_RejectsDuplicateUnorderedRoutes()
        {
            var a = ContentId.From("a");
            var b = ContentId.From("b");
            var nodes = new[]
            {
                new MapNode(a, MapNodeKind.System, Vector2.zero, null),
                new MapNode(b, MapNodeKind.System, Vector2.one, null),
            };
            var routes = new[]
            {
                new MapRoute(a, b, 3f),
                new MapRoute(b, a, 9f),
            };

            Assert.That(MapGraph.TryCreate(nodes, routes, out var graph, out var error), Is.False);
            Assert.That(graph, Is.Null);
            Assert.That(error, Does.Contain("Duplicate unordered route"));
        }

        [Test]
        public void TryCreate_RejectsUnknownEndpoint()
        {
            var nodes = new[]
            {
                new MapNode(ContentId.From("a"), MapNodeKind.System, Vector2.zero, null),
            };
            var routes = new[]
            {
                new MapRoute(ContentId.From("a"), ContentId.From("missing"), 2f),
            };

            Assert.That(MapGraph.TryCreate(nodes, routes, out _, out var error), Is.False);
            Assert.That(error, Does.Contain("unknown node"));
        }

        [Test]
        public void TryCreate_RejectsDuplicateNodeIds()
        {
            var id = ContentId.From("a");
            var nodes = new[]
            {
                new MapNode(id, MapNodeKind.System, Vector2.zero, null),
                new MapNode(id, MapNodeKind.NonSystem, Vector2.one, null),
            };

            Assert.That(MapGraph.TryCreate(nodes, null, out _, out var error), Is.False);
            Assert.That(error, Does.Contain("Duplicate map node id"));
        }
    }
}
