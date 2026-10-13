using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Universe;

namespace EndlessHeros.Overworld.Services
{
    /// <summary>
    /// Stateless route queries against the universe map graph.
    /// </summary>
    public sealed class RouteService
    {
        /// <summary>
        /// Returns neighbor ids for a node, or empty when unknown.
        /// </summary>
        public IReadOnlyList<ContentId> GetNeighbors(MapGraph graph, ContentId nodeId)
        {
            if (graph == null)
            {
                return System.Array.Empty<ContentId>();
            }

            return graph.GetNeighbors(nodeId);
        }

        /// <summary>
        /// Returns whether two nodes share a route.
        /// </summary>
        public bool AreAdjacent(MapGraph graph, ContentId left, ContentId right)
        {
            return graph != null && graph.AreAdjacent(left, right);
        }

        /// <summary>
        /// Looks up travel duration for an adjacent pair.
        /// </summary>
        public bool TryGetTravelDuration(MapGraph graph, ContentId left, ContentId right, out float durationSeconds)
        {
            durationSeconds = 0f;
            if (graph == null || !graph.TryGetRoute(left, right, out var route))
            {
                return false;
            }

            durationSeconds = route.TravelDurationSeconds;
            return true;
        }

        /// <summary>
        /// Stable key for undirected route knowledge storage.
        /// </summary>
        public static ContentId MakeRouteKey(ContentId left, ContentId right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return default;
            }

            var a = left.Value;
            var b = right.Value;
            if (string.CompareOrdinal(a, b) > 0)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            return ContentId.From(a + "|" + b);
        }
    }
}
