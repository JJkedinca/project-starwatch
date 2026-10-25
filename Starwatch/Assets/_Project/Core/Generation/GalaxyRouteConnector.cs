using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Builds an undirected route set from map positions: MST for connectivity, then cheapest
    /// fill until every node has degree at least 2 (capped at 6). Shape
    /// <see cref="GalaxyShapeSO.RouteCost"/> and route groups are enforced.
    /// <see cref="GalaxyShapeSO.MinOutwardRoutes"/> waits for ring assignment — not applied here.
    /// Travel times scale so the median kept edge hits a target duration.
    /// </summary>
    internal static class GalaxyRouteConnector
    {
        private const int kMinDegree = 2;
        private const int kMaxDegree = 6;
        private const float kMinTravelSeconds = 1f;

        /// <summary>
        /// Connects <paramref name="nodeIds"/> into routes using map lengths for travel time and
        /// unit-box positions on <paramref name="layoutNodes"/> for shape cost/group queries.
        /// </summary>
        public static bool TryConnect(
            IReadOnlyList<ContentId> nodeIds,
            IReadOnlyList<Vector2> mapPositions,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            GalaxyShapeSO shape,
            float targetMedianTravelSeconds,
            out List<MapRouteDefinition> routes)
        {
            routes = null;
            if (shape == null
                || nodeIds == null
                || mapPositions == null
                || layoutNodes == null
                || nodeIds.Count != mapPositions.Count
                || nodeIds.Count != layoutNodes.Count
                || nodeIds.Count < 2)
            {
                Debug.LogError(
                    "GalaxyRouteConnector.TryConnect requires a shape and at least two nodes with matching positions.");
                return false;
            }

            var candidates = BuildCandidates(mapPositions, layoutNodes, shape);
            if (candidates.Count == 0)
            {
                Debug.LogError("GalaxyRouteConnector.TryConnect has no legal candidate edges after shape costs.");
                return false;
            }

            candidates.Sort(static (a, b) => a.Cost.CompareTo(b.Cost));

            var parent = new int[nodeIds.Count];
            for (var i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
            }

            var degree = new int[nodeIds.Count];
            var kept = new List<CandidateEdge>(nodeIds.Count * 2);
            var keptKeys = new HashSet<long>();
            var groupCounts = new Dictionary<int, int>();

            for (var i = 0; i < candidates.Count; i++)
            {
                var edge = candidates[i];
                var rootA = Find(parent, edge.IndexA);
                var rootB = Find(parent, edge.IndexB);
                if (rootA == rootB)
                {
                    continue;
                }

                if (!CanKeepGroup(edge, shape, layoutNodes, groupCounts))
                {
                    continue;
                }

                parent[rootB] = rootA;
                AddKept(edge, kept, keptKeys, degree, shape, layoutNodes, groupCounts);
                if (kept.Count == nodeIds.Count - 1)
                {
                    break;
                }
            }

            if (kept.Count != nodeIds.Count - 1)
            {
                Debug.LogError("GalaxyRouteConnector.TryConnect failed to build a spanning tree.");
                return false;
            }

            FillMinDegree(candidates, kept, keptKeys, degree, shape, layoutNodes, groupCounts);

            if (!AllMeetMinDegree(degree))
            {
                Debug.LogError("GalaxyRouteConnector.TryConnect finished below minimum node degree.");
                return false;
            }

            routes = BuildTravelRoutes(nodeIds, kept, targetMedianTravelSeconds);
            return true;
        }

        private static List<CandidateEdge> BuildCandidates(
            IReadOnlyList<Vector2> mapPositions,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            GalaxyShapeSO shape)
        {
            var candidates = new List<CandidateEdge>(layoutNodes.Count * (layoutNodes.Count - 1) / 2);
            for (var i = 0; i < layoutNodes.Count; i++)
            {
                for (var j = i + 1; j < layoutNodes.Count; j++)
                {
                    var multiplier = shape.RouteCost(layoutNodes[i].Position, layoutNodes[j].Position);
                    if (float.IsInfinity(multiplier) || float.IsNaN(multiplier) || multiplier <= 0f)
                    {
                        continue;
                    }

                    var length = Vector2.Distance(mapPositions[i], mapPositions[j]);
                    candidates.Add(new CandidateEdge(i, j, length, length * multiplier));
                }
            }

            return candidates;
        }

        private static void FillMinDegree(
            IReadOnlyList<CandidateEdge> candidates,
            List<CandidateEdge> kept,
            HashSet<long> keptKeys,
            int[] degree,
            GalaxyShapeSO shape,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            Dictionary<int, int> groupCounts)
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                if (AllMeetMinDegree(degree))
                {
                    return;
                }

                var edge = candidates[i];
                if (keptKeys.Contains(EdgeKey(edge.IndexA, edge.IndexB)))
                {
                    continue;
                }

                if (degree[edge.IndexA] >= kMaxDegree || degree[edge.IndexB] >= kMaxDegree)
                {
                    continue;
                }

                if (degree[edge.IndexA] >= kMinDegree && degree[edge.IndexB] >= kMinDegree)
                {
                    continue;
                }

                if (!CanKeepGroup(edge, shape, layoutNodes, groupCounts))
                {
                    continue;
                }

                AddKept(edge, kept, keptKeys, degree, shape, layoutNodes, groupCounts);
            }
        }

        private static List<MapRouteDefinition> BuildTravelRoutes(
            IReadOnlyList<ContentId> nodeIds,
            IReadOnlyList<CandidateEdge> kept,
            float targetMedianTravelSeconds)
        {
            var lengths = new float[kept.Count];
            for (var i = 0; i < kept.Count; i++)
            {
                lengths[i] = kept[i].Length;
            }

            Array.Sort(lengths);
            var medianLength = lengths[lengths.Length / 2];
            if (medianLength <= 0f)
            {
                medianLength = 1f;
            }

            var target = Mathf.Max(kMinTravelSeconds, targetMedianTravelSeconds);
            var secondsPerUnit = target / medianLength;

            var routes = new List<MapRouteDefinition>(kept.Count);
            for (var i = 0; i < kept.Count; i++)
            {
                var edge = kept[i];
                var travel = Mathf.Max(kMinTravelSeconds, edge.Length * secondsPerUnit);
                routes.Add(new MapRouteDefinition(nodeIds[edge.IndexA], nodeIds[edge.IndexB], travel));
            }

            return routes;
        }

        private static bool CanKeepGroup(
            CandidateEdge edge,
            GalaxyShapeSO shape,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            Dictionary<int, int> groupCounts)
        {
            if (!shape.TryGetRouteGroup(
                    layoutNodes[edge.IndexA].Position,
                    layoutNodes[edge.IndexB].Position,
                    out var groupId,
                    out var maxRoutes))
            {
                return true;
            }

            if (maxRoutes <= 0)
            {
                return false;
            }

            return !groupCounts.TryGetValue(groupId, out var keptInGroup) || keptInGroup < maxRoutes;
        }

        private static void AddKept(
            CandidateEdge edge,
            List<CandidateEdge> kept,
            HashSet<long> keptKeys,
            int[] degree,
            GalaxyShapeSO shape,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            Dictionary<int, int> groupCounts)
        {
            kept.Add(edge);
            keptKeys.Add(EdgeKey(edge.IndexA, edge.IndexB));
            degree[edge.IndexA]++;
            degree[edge.IndexB]++;

            if (shape.TryGetRouteGroup(
                    layoutNodes[edge.IndexA].Position,
                    layoutNodes[edge.IndexB].Position,
                    out var groupId,
                    out _))
            {
                groupCounts.TryGetValue(groupId, out var count);
                groupCounts[groupId] = count + 1;
            }
        }

        private static bool AllMeetMinDegree(IReadOnlyList<int> degree)
        {
            for (var i = 0; i < degree.Count; i++)
            {
                if (degree[i] < kMinDegree)
                {
                    return false;
                }
            }

            return true;
        }

        private static int Find(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }

            return index;
        }

        private static long EdgeKey(int a, int b)
        {
            if (a > b)
            {
                (a, b) = (b, a);
            }

            return ((long)a << 32) | (uint)b;
        }

        private readonly struct CandidateEdge
        {
            public int IndexA { get; }

            public int IndexB { get; }

            public float Length { get; }

            public float Cost { get; }

            public CandidateEdge(int indexA, int indexB, float length, float cost)
            {
                IndexA = indexA;
                IndexB = indexB;
                Length = length;
                Cost = cost;
            }
        }
    }
}
