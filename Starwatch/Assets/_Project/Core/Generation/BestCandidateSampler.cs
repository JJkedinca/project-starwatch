using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Draws density-biased points in the unit box and keeps the one farthest from existing nodes.
    /// Spreads the map evenly while still preferring arms, lanes, and other high-density regions.
    /// </summary>
    internal static class BestCandidateSampler
    {
        private const int kMaxSampleAttemptsPerCandidate = 64;
        private const float kTieEpsilon = 1e-6f;

        /// <summary>
        /// Draws <paramref name="candidateCount"/> density-accepted samples that clear
        /// <paramref name="minSpacing"/> from existing nodes, then keeps the one with the largest
        /// clearance. Near-ties break toward higher density.
        /// </summary>
        public static bool TrySample(
            GalaxyShapeSO shape,
            IReadOnlyList<Vector2> existing,
            int candidateCount,
            float minSpacing,
            System.Random random,
            out Vector2 bestPoint)
        {
            bestPoint = default;
            if (shape == null || existing == null || random == null || candidateCount < 1)
            {
                return false;
            }

            var spacing = Mathf.Max(0f, minSpacing);
            var found = false;
            var bestMinDistance = float.NegativeInfinity;
            var bestDensity = float.NegativeInfinity;

            for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
            {
                if (!TryDrawAcceptedPoint(shape, random, out var point))
                {
                    continue;
                }

                var minDistance = MinDistanceToExisting(point, existing);
                if (existing.Count > 0 && minDistance < spacing)
                {
                    continue;
                }

                var density = shape.Density(point);
                if (!found
                    || minDistance > bestMinDistance + kTieEpsilon
                    || (Mathf.Abs(minDistance - bestMinDistance) <= kTieEpsilon && density > bestDensity))
                {
                    found = true;
                    bestMinDistance = minDistance;
                    bestDensity = density;
                    bestPoint = point;
                }
            }

            return found;
        }

        /// <summary>
        /// Rejection-samples the unit box: keep a uniform point with probability equal to its
        /// density. Density 0 is never accepted; denser regions win more draws.
        /// </summary>
        private static bool TryDrawAcceptedPoint(GalaxyShapeSO shape, System.Random random, out Vector2 point)
        {
            for (var attempt = 0; attempt < kMaxSampleAttemptsPerCandidate; attempt++)
            {
                point = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                var density = shape.Density(point);
                if (density <= 0f)
                {
                    continue;
                }

                if (random.NextDouble() <= density)
                {
                    return true;
                }
            }

            point = default;
            return false;
        }

        private static float MinDistanceToExisting(Vector2 point, IReadOnlyList<Vector2> existing)
        {
            var minDistance = float.PositiveInfinity;
            for (var i = 0; i < existing.Count; i++)
            {
                var distance = Vector2.Distance(point, existing[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }
    }
}
