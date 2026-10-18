using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Universe;

namespace ProjectStarwatch.Overworld.Services
{
    /// <summary>
    /// Stateless route queries against the universe map graph.
    /// </summary>
    internal sealed class RouteService
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
            return MapRoute.MakeKnowledgeKey(left, right);
        }
    }
}
