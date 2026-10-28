using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Builds routes from Delaunay neighbor candidates: MST for connectivity, then fill toward
    /// degree 2 until only a leaf budget remains. Shape <see cref="GalaxyShapeSO.RouteCost"/>
    /// and route groups are enforced. Long Delaunay edges are pruned before fill.
    /// </summary>
    internal static class GalaxyRouteConnector
    {
        /// <summary>Connectivity floor after MST. Leaves are allowed.</summary>
        private const int kMinDegree = 1;

        /// <summary>Soft fill target for non-budgeted nodes.</summary>
        private const int kFillTargetDegree = 2;

        /// <summary>Do not fill a node past this during the fill pass.</summary>
        private const int kFillCapDegree = 4;

        /// <summary>
        /// Fraction of nodes that may remain degree-1 after fill. Fill stops once leaf count
        /// is at or below this budget (leaves are intentional dead-ends, not failures).
        /// </summary>
        private const float kLeafBudgetFraction = 0.15f;

        private const int kMaxDegree = 6;
        private const float kLongEdgeMedianMultiple = 2.5f;
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

            var unitPositions = new Vector2[layoutNodes.Count];
            for (var i = 0; i < layoutNodes.Count; i++)
            {
                unitPositions[i] = layoutNodes[i].Position;
            }

            var delaunay = DelaunayTriangulator.BuildEdges(unitPositions);
            var mstCandidates = BuildCostedCandidates(delaunay, mapPositions, layoutNodes, shape, pruneLongEdges: false);
            if (mstCandidates.Count == 0)
            {
                Debug.LogError("GalaxyRouteConnector.TryConnect has no legal Delaunay candidates after shape costs.");
                return false;
            }

            mstCandidates.Sort(static (a, b) => a.Cost.CompareTo(b.Cost));

            var parent = new int[nodeIds.Count];
            for (var i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
            }

            var degree = new int[nodeIds.Count];
            var kept = new List<CandidateEdge>(nodeIds.Count * 2);
            var keptKeys = new HashSet<long>();
            var groupCounts = new Dictionary<int, int>();

            for (var i = 0; i < mstCandidates.Count; i++)
            {
                var edge = mstCandidates[i];
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

            var fillCandidates = BuildCostedCandidates(delaunay, mapPositions, layoutNodes, shape, pruneLongEdges: true);
            fillCandidates.Sort(static (a, b) => a.Cost.CompareTo(b.Cost));
            FillTowardTarget(
                fillCandidates,
                kept,
                keptKeys,
                degree,
                shape,
                layoutNodes,
                groupCounts,
                MaxLeafBudget(nodeIds.Count));

            if (!AllMeetMinDegree(degree))
            {
                Debug.LogError("GalaxyRouteConnector.TryConnect finished below minimum node degree.");
                return false;
            }

            routes = BuildTravelRoutes(nodeIds, kept, targetMedianTravelSeconds);
            return true;
        }

        private static List<CandidateEdge> BuildCostedCandidates(
            IReadOnlyList<DelaunayTriangulator.Edge> delaunayEdges,
            IReadOnlyList<Vector2> mapPositions,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            GalaxyShapeSO shape,
            bool pruneLongEdges)
        {
            var raw = new List<CandidateEdge>(delaunayEdges.Count);
            for (var i = 0; i < delaunayEdges.Count; i++)
            {
                var pair = delaunayEdges[i];
                var multiplier = shape.RouteCost(layoutNodes[pair.IndexA].Position, layoutNodes[pair.IndexB].Position);
                if (float.IsInfinity(multiplier) || float.IsNaN(multiplier) || multiplier <= 0f)
                {
                    continue;
                }

                var length = Vector2.Distance(mapPositions[pair.IndexA], mapPositions[pair.IndexB]);
                raw.Add(new CandidateEdge(pair.IndexA, pair.IndexB, length, length * multiplier));
            }

            if (!pruneLongEdges || raw.Count == 0)
            {
                return raw;
            }

            var lengths = new float[raw.Count];
            for (var i = 0; i < raw.Count; i++)
            {
                lengths[i] = raw[i].Length;
            }

            Array.Sort(lengths);
            var median = lengths[lengths.Length / 2];
            if (median <= 0f)
            {
                return raw;
            }

            var maxLength = median * kLongEdgeMedianMultiple;
            var pruned = new List<CandidateEdge>(raw.Count);
            for (var i = 0; i < raw.Count; i++)
            {
                if (raw[i].Length <= maxLength)
                {
                    pruned.Add(raw[i]);
                }
            }

            return pruned;
        }

        private static void FillTowardTarget(
            IReadOnlyList<CandidateEdge> candidates,
            List<CandidateEdge> kept,
            HashSet<long> keptKeys,
            int[] degree,
            GalaxyShapeSO shape,
            IReadOnlyList<GeneratedGalaxyNode> layoutNodes,
            Dictionary<int, int> groupCounts,
            int maxLeaves)
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                if (CountLeaves(degree) <= maxLeaves)
                {
                    return;
                }

                if (AllMeetFillTarget(degree))
                {
                    return;
                }

                var edge = candidates[i];
                if (keptKeys.Contains(EdgeKey(edge.IndexA, edge.IndexB)))
                {
                    continue;
                }

                if (degree[edge.IndexA] >= kFillCapDegree || degree[edge.IndexB] >= kFillCapDegree)
                {
                    continue;
                }

                if (degree[edge.IndexA] >= kMaxDegree || degree[edge.IndexB] >= kMaxDegree)
                {
                    continue;
                }

                // Only add edges that raise an under-target (leaf) endpoint.
                var helpsA = degree[edge.IndexA] < kFillTargetDegree;
                var helpsB = degree[edge.IndexB] < kFillTargetDegree;
                if (!helpsA && !helpsB)
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

        private static int MaxLeafBudget(int nodeCount)
        {
            if (nodeCount <= 0)
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.FloorToInt(nodeCount * kLeafBudgetFraction));
        }

        private static int CountLeaves(IReadOnlyList<int> degree)
        {
            var leaves = 0;
            for (var i = 0; i < degree.Count; i++)
            {
                if (degree[i] < kFillTargetDegree)
                {
                    leaves++;
                }
            }

            return leaves;
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

        private static bool AllMeetFillTarget(IReadOnlyList<int> degree)
        {
            for (var i = 0; i < degree.Count; i++)
            {
                if (degree[i] < kFillTargetDegree)
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
