using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Universe
{
    /// <summary>
    /// Immutable universe map: nodes, undirected routes, and adjacency lookup.
    /// </summary>
    public sealed class MapGraph
    {
        public static MapGraph Empty { get; } = CreateEmpty();

        public IEnumerable<MapNode> Nodes => _nodesById.Values;

        public int NodeCount => _nodesById.Count;

        public IReadOnlyList<MapRoute> Routes => _routes;

        private readonly List<MapRoute> _routes;
        private readonly Dictionary<ContentId, MapNode> _nodesById;
        private readonly Dictionary<ContentId, List<ContentId>> _adjacency;

        private MapGraph(
            List<MapRoute> routes,
            Dictionary<ContentId, MapNode> nodesById,
            Dictionary<ContentId, List<ContentId>> adjacency)
        {
            _routes = routes;
            _nodesById = nodesById;
            _adjacency = adjacency;
        }

        /// <summary>
        /// Builds a graph when every node and route is valid. Rejects duplicates,
        /// missing endpoints, self-loops, and negative durations.
        /// </summary>
        public static bool TryCreate(
            IReadOnlyList<MapNode> nodes,
            IReadOnlyList<MapRoute> routes,
            out MapGraph graph,
            out string error)
        {
            graph = null;
            error = null;

            List<MapRoute> builtRoutes = new();
            Dictionary<ContentId, MapNode> nodesById = new();
            Dictionary<ContentId, List<ContentId>> adjacency = new();

            if (nodes != null)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node == null)
                    {
                        error = $"Map node at index {i} is null.";
                        return false;
                    }

                    if (node.Id.IsEmpty)
                    {
                        error = $"Map node at index {i} has an empty id.";
                        return false;
                    }

                    if (nodesById.ContainsKey(node.Id))
                    {
                        error = $"Duplicate map node id '{node.Id.Value}'.";
                        return false;
                    }

                    nodesById.Add(node.Id, node);
                    adjacency[node.Id] = new List<ContentId>();
                }
            }

            if (routes != null)
            {
                for (var i = 0; i < routes.Count; i++)
                {
                    var route = routes[i];
                    if (route == null)
                    {
                        error = $"Map route at index {i} is null.";
                        return false;
                    }

                    if (route.NodeAId.IsEmpty || route.NodeBId.IsEmpty)
                    {
                        error = $"Map route at index {i} has an empty endpoint.";
                        return false;
                    }

                    if (route.NodeAId == route.NodeBId)
                    {
                        error = $"Map route at index {i} is a self-loop on '{route.NodeAId.Value}'.";
                        return false;
                    }

                    if (route.TravelDurationSeconds < 0f || float.IsNaN(route.TravelDurationSeconds))
                    {
                        error = $"Map route at index {i} has a negative or invalid duration.";
                        return false;
                    }

                    if (!nodesById.ContainsKey(route.NodeAId))
                    {
                        error = $"Map route at index {i} references unknown node '{route.NodeAId.Value}'.";
                        return false;
                    }

                    if (!nodesById.ContainsKey(route.NodeBId))
                    {
                        error = $"Map route at index {i} references unknown node '{route.NodeBId.Value}'.";
                        return false;
                    }

                    if (adjacency[route.NodeAId].Contains(route.NodeBId))
                    {
                        error =
                            $"Duplicate unordered route between '{route.NodeAId.Value}' and '{route.NodeBId.Value}'.";
                        return false;
                    }

                    builtRoutes.Add(route);
                    adjacency[route.NodeAId].Add(route.NodeBId);
                    adjacency[route.NodeBId].Add(route.NodeAId);
                }
            }

            graph = new MapGraph(builtRoutes, nodesById, adjacency);
            return true;
        }

        /// <summary>
        /// Returns whether every node is reachable from every other node.
        /// </summary>
        public bool IsFullyConnected()
        {
            if (_nodesById.Count <= 1)
            {
                return true;
            }

            ContentId start = default;
            foreach (var id in _nodesById.Keys)
            {
                start = id;
                break;
            }

            HashSet<ContentId> visited = new();
            Queue<ContentId> queue = new();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = _adjacency[current];
                for (var i = 0; i < neighbors.Count; i++)
                {
                    var next = neighbors[i];
                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            return visited.Count == _nodesById.Count;
        }

        /// <summary>
        /// Looks up a node by id.
        /// </summary>
        public bool TryGetNode(ContentId nodeId, out MapNode node)
        {
            return _nodesById.TryGetValue(nodeId, out node);
        }

        /// <summary>
        /// Returns neighbor node ids, or empty when the node is unknown.
        /// </summary>
        public IReadOnlyList<ContentId> GetNeighbors(ContentId nodeId)
        {
            if (_adjacency.TryGetValue(nodeId, out var neighbors))
            {
                return neighbors;
            }

            return Array.Empty<ContentId>();
        }

        /// <summary>
        /// Returns whether an undirected edge exists between the two nodes.
        /// </summary>
        public bool AreAdjacent(ContentId left, ContentId right)
        {
            if (!_adjacency.TryGetValue(left, out var neighbors))
            {
                return false;
            }

            return neighbors.Contains(right);
        }

        /// <summary>
        /// Finds the route connecting two adjacent nodes.
        /// </summary>
        public bool TryGetRoute(ContentId left, ContentId right, out MapRoute route)
        {
            for (var i = 0; i < _routes.Count; i++)
            {
                if (_routes[i].Connects(left, right))
                {
                    route = _routes[i];
                    return true;
                }
            }

            route = null;
            return false;
        }

        /// <summary>
        /// Finds the node that contains the given planet id.
        /// </summary>
        public bool TryGetNodeForPlanet(ContentId planetId, out MapNode node)
        {
            node = null;
            if (planetId.IsEmpty)
            {
                return false;
            }

            foreach (var candidate in _nodesById.Values)
            {
                for (var p = 0; p < candidate.PlanetIds.Count; p++)
                {
                    if (candidate.PlanetIds[p] == planetId)
                    {
                        node = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static MapGraph CreateEmpty()
        {
            TryCreate(null, null, out var graph, out _);
            return graph;
        }
    }
}
