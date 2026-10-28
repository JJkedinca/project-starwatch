using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// 2D Delaunay triangulation (Bowyer–Watson) for small point sets.
    /// Returns undirected edges as sorted index pairs suitable for route candidates.
    /// </summary>
    internal static class DelaunayTriangulator
    {
        private const float kEpsilon = 1e-8f;

        /// <summary>
        /// Builds unique undirected edges of the Delaunay triangulation of <paramref name="points"/>.
        /// For fewer than three points, returns the complete graph on those points.
        /// Collinear (or otherwise degenerate) sets fall back to adjacent links along the principal axis.
        /// </summary>
        public static List<Edge> BuildEdges(IReadOnlyList<Vector2> points)
        {
            var edges = new List<Edge>();
            if (points == null || points.Count < 2)
            {
                return edges;
            }

            if (points.Count == 2)
            {
                edges.Add(Edge.Sorted(0, 1));
                return edges;
            }

            if (AreCollinear(points))
            {
                return BuildSortedAdjacentChain(points);
            }

            if (points.Count == 3)
            {
                edges.Add(Edge.Sorted(0, 1));
                edges.Add(Edge.Sorted(1, 2));
                edges.Add(Edge.Sorted(0, 2));
                return edges;
            }

            var super = CreateSuperTriangle(points);
            var triangles = new List<Triangle>
            {
                new(points.Count, points.Count + 1, points.Count + 2),
            };

            var allPoints = new Vector2[points.Count + 3];
            for (var i = 0; i < points.Count; i++)
            {
                allPoints[i] = points[i];
            }

            allPoints[points.Count] = super.A;
            allPoints[points.Count + 1] = super.B;
            allPoints[points.Count + 2] = super.C;

            for (var pointIndex = 0; pointIndex < points.Count; pointIndex++)
            {
                InsertPoint(pointIndex, allPoints, triangles);
            }

            var edgeKeys = new HashSet<long>();
            for (var i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                if (triangle.SharesVertexWithSuper(points.Count))
                {
                    continue;
                }

                TryAddEdge(edges, edgeKeys, triangle.A, triangle.B);
                TryAddEdge(edges, edgeKeys, triangle.B, triangle.C);
                TryAddEdge(edges, edgeKeys, triangle.C, triangle.A);
            }

            // Degenerate numeric cases can still yield an empty or disconnected edge set.
            if (edges.Count < points.Count - 1)
            {
                return BuildSortedAdjacentChain(points);
            }

            return edges;
        }

        /// <summary>
        /// Connects points in order of projection onto their longest axis (adjacent chain).
        /// </summary>
        private static List<Edge> BuildSortedAdjacentChain(IReadOnlyList<Vector2> points)
        {
            var order = new int[points.Count];
            for (var i = 0; i < order.Length; i++)
            {
                order[i] = i;
            }

            var axis = PrincipalAxis(points);
            System.Array.Sort(order, (left, right) =>
            {
                var projectionLeft = Vector2.Dot(points[left], axis);
                var projectionRight = Vector2.Dot(points[right], axis);
                return projectionLeft.CompareTo(projectionRight);
            });

            var edges = new List<Edge>(points.Count - 1);
            for (var i = 0; i < order.Length - 1; i++)
            {
                edges.Add(Edge.Sorted(order[i], order[i + 1]));
            }

            return edges;
        }

        private static bool AreCollinear(IReadOnlyList<Vector2> points)
        {
            if (points.Count < 3)
            {
                return true;
            }

            var origin = points[0];
            Vector2 direction = default;
            var hasDirection = false;
            for (var i = 1; i < points.Count; i++)
            {
                var delta = points[i] - origin;
                if (delta.sqrMagnitude <= kEpsilon)
                {
                    continue;
                }

                if (!hasDirection)
                {
                    direction = delta.normalized;
                    hasDirection = true;
                    continue;
                }

                var cross = direction.x * delta.y - direction.y * delta.x;
                if (Mathf.Abs(cross) > kEpsilon * 10f)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector2 PrincipalAxis(IReadOnlyList<Vector2> points)
        {
            var minIndex = 0;
            var maxIndex = 0;
            var bestSqr = -1f;
            for (var i = 0; i < points.Count; i++)
            {
                for (var j = i + 1; j < points.Count; j++)
                {
                    var sqr = (points[j] - points[i]).sqrMagnitude;
                    if (sqr <= bestSqr)
                    {
                        continue;
                    }

                    bestSqr = sqr;
                    minIndex = i;
                    maxIndex = j;
                }
            }

            var axis = points[maxIndex] - points[minIndex];
            return axis.sqrMagnitude <= kEpsilon ? Vector2.right : axis.normalized;
        }

        private static void InsertPoint(int pointIndex, IReadOnlyList<Vector2> allPoints, List<Triangle> triangles)
        {
            var bad = new List<int>();
            for (var i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].CircumcircleContains(allPoints[pointIndex], allPoints))
                {
                    bad.Add(i);
                }
            }

            var polygon = new Dictionary<long, int>();
            for (var i = 0; i < bad.Count; i++)
            {
                var triangle = triangles[bad[i]];
                AddBoundaryEdge(polygon, triangle.A, triangle.B);
                AddBoundaryEdge(polygon, triangle.B, triangle.C);
                AddBoundaryEdge(polygon, triangle.C, triangle.A);
            }

            for (var i = bad.Count - 1; i >= 0; i--)
            {
                triangles.RemoveAt(bad[i]);
            }

            foreach (var pair in polygon)
            {
                if (pair.Value != 1)
                {
                    continue;
                }

                DecodeEdge(pair.Key, out var a, out var b);
                triangles.Add(new Triangle(a, b, pointIndex));
            }
        }

        private static void AddBoundaryEdge(Dictionary<long, int> polygon, int a, int b)
        {
            var key = EdgeKey(a, b);
            polygon.TryGetValue(key, out var count);
            polygon[key] = count + 1;
        }

        private static void TryAddEdge(List<Edge> edges, HashSet<long> edgeKeys, int a, int b)
        {
            var edge = Edge.Sorted(a, b);
            if (!edgeKeys.Add(EdgeKey(edge.IndexA, edge.IndexB)))
            {
                return;
            }

            edges.Add(edge);
        }

        private static SuperTriangle CreateSuperTriangle(IReadOnlyList<Vector2> points)
        {
            var minX = points[0].x;
            var minY = points[0].y;
            var maxX = minX;
            var maxY = minY;
            for (var i = 1; i < points.Count; i++)
            {
                var point = points[i];
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }

            var dx = maxX - minX;
            var dy = maxY - minY;
            var delta = Mathf.Max(dx, dy, 1f) * 10f;
            var midX = (minX + maxX) * 0.5f;
            var midY = (minY + maxY) * 0.5f;
            return new SuperTriangle(
                new Vector2(midX - delta, midY - delta),
                new Vector2(midX + delta, midY - delta),
                new Vector2(midX, midY + delta));
        }

        private static long EdgeKey(int a, int b)
        {
            if (a > b)
            {
                (a, b) = (b, a);
            }

            return ((long)a << 32) | (uint)b;
        }

        private static void DecodeEdge(long key, out int a, out int b)
        {
            a = (int)(key >> 32);
            b = (int)(key & 0xffffffffL);
        }

        internal readonly struct Edge
        {
            public int IndexA { get; }

            public int IndexB { get; }

            public Edge(int indexA, int indexB)
            {
                IndexA = indexA;
                IndexB = indexB;
            }

            public static Edge Sorted(int a, int b)
            {
                return a <= b ? new Edge(a, b) : new Edge(b, a);
            }
        }

        private readonly struct SuperTriangle
        {
            public Vector2 A { get; }

            public Vector2 B { get; }

            public Vector2 C { get; }

            public SuperTriangle(Vector2 a, Vector2 b, Vector2 c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        private readonly struct Triangle
        {
            public int A { get; }

            public int B { get; }

            public int C { get; }

            public Triangle(int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;
            }

            public bool SharesVertexWithSuper(int firstSuperIndex)
            {
                return A >= firstSuperIndex || B >= firstSuperIndex || C >= firstSuperIndex;
            }

            public bool CircumcircleContains(Vector2 point, IReadOnlyList<Vector2> allPoints)
            {
                var ax = allPoints[A].x;
                var ay = allPoints[A].y;
                var bx = allPoints[B].x;
                var by = allPoints[B].y;
                var cx = allPoints[C].x;
                var cy = allPoints[C].y;

                var adx = ax - point.x;
                var ady = ay - point.y;
                var bdx = bx - point.x;
                var bdy = by - point.y;
                var cdx = cx - point.x;
                var cdy = cy - point.y;

                var abdet = adx * bdy - bdx * ady;
                var bcdet = bdx * cdy - cdx * bdy;
                var cadet = cdx * ady - adx * cdy;
                var alift = adx * adx + ady * ady;
                var blift = bdx * bdx + bdy * bdy;
                var clift = cdx * cdx + cdy * cdy;

                var det = alift * bcdet + blift * cadet + clift * abdet;
                var orient = Orient2D(ax, ay, bx, by, cx, cy);
                if (orient < 0f)
                {
                    det = -det;
                }

                return det > kEpsilon;
            }

            private static float Orient2D(float ax, float ay, float bx, float by, float cx, float cy)
            {
                return (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            }
        }
    }
}
