using System.Collections.Generic;
using NUnit.Framework;
using ProjectStarwatch.Core.Generation;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.Generation
{
    /// <summary>
    /// EditMode coverage for 2D Delaunay edge extraction.
    /// </summary>
    public sealed class DelaunayTriangulatorTests
    {
        [Test]
        public void BuildEdges_TwoPoints_ReturnsSingleEdge()
        {
            var edges = DelaunayTriangulator.BuildEdges(new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
            });

            Assert.That(edges.Count, Is.EqualTo(1));
            Assert.That(HasEdge(edges, 0, 1), Is.True);
        }

        [Test]
        public void BuildEdges_CollinearPoints_ReturnsAdjacentChain()
        {
            var edges = DelaunayTriangulator.BuildEdges(new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0.75f, 0f),
                new Vector2(0.25f, 0f),
                new Vector2(1f, 0f),
            });

            Assert.That(edges.Count, Is.EqualTo(3));
            Assert.That(HasEdge(edges, 0, 2), Is.True);
            Assert.That(HasEdge(edges, 2, 1), Is.True);
            Assert.That(HasEdge(edges, 1, 3), Is.True);
            Assert.That(HasEdge(edges, 0, 3), Is.False);
        }

        [Test]
        public void BuildEdges_Rectangle_KeepsBoundaryAndOneDiagonal()
        {
            // Slightly non-square so points are not cocircular.
            var edges = DelaunayTriangulator.BuildEdges(new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1.2f, 0f),
                new Vector2(1.2f, 1f),
                new Vector2(0f, 1f),
            });

            Assert.That(HasEdge(edges, 0, 1), Is.True);
            Assert.That(HasEdge(edges, 1, 2), Is.True);
            Assert.That(HasEdge(edges, 2, 3), Is.True);
            Assert.That(HasEdge(edges, 3, 0), Is.True);

            var diagonalCount = 0;
            if (HasEdge(edges, 0, 2))
            {
                diagonalCount++;
            }

            if (HasEdge(edges, 1, 3))
            {
                diagonalCount++;
            }

            Assert.That(diagonalCount, Is.EqualTo(1));
        }

        [Test]
        public void BuildEdges_ClusterPlusOutlier_IncludesLocalNeighbors()
        {
            var edges = DelaunayTriangulator.BuildEdges(new[]
            {
                new Vector2(0.2f, 0.2f),
                new Vector2(0.3f, 0.25f),
                new Vector2(0.25f, 0.35f),
                new Vector2(0.8f, 0.8f),
            });

            Assert.That(HasEdge(edges, 0, 1), Is.True);
            Assert.That(HasEdge(edges, 0, 2) || HasEdge(edges, 1, 2), Is.True);
        }

        private static bool HasEdge(IReadOnlyList<DelaunayTriangulator.Edge> edges, int a, int b)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                if ((edge.IndexA == a && edge.IndexB == b) || (edge.IndexA == b && edge.IndexB == a))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
