using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Shared best-candidate node placement for density-driven galaxy shapes.
    /// Callers retry with a new random stream when this returns false.
    /// </summary>
    internal static class GalaxyNodePlacer
    {
        /// <summary>Round sampled positions to three decimals so tiny float noise does not change later triangulation.</summary>
        private const float kPositionQuantum = 0.001f;

        /// <summary>Design floor: every sector needs enough nodes to feel like a real expansion direction.</summary>
        private const int kMinNodesPerSector = 3;

        /// <summary>
        /// Places a variance-rolled node count in unit-box space, then picks home uniformly among them.
        /// Fails (caller should retry) when sampling stalls, min spacing is broken after rounding,
        /// or any sector ends below <see cref="kMinNodesPerSector"/> nodes.
        /// </summary>
        public static bool TryPlace(GalaxyShapeSO shape, GenerationSettingsSO settings, System.Random random, out GeneratedGalaxyLayout layout)
        {
            layout = null;
            if (!TryPrepare(shape, settings, random, out var targetCount))
            {
                return false;
            }

            if (!TryGenerate(shape, settings, random, targetCount, out layout))
            {
                return false;
            }

            if (!TryValidate(layout, shape, settings))
            {
                layout = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks inputs, rolls shape-local randomness, and picks the node count for this attempt.
        /// </summary>
        private static bool TryPrepare(
            GalaxyShapeSO shape,
            GenerationSettingsSO settings,
            System.Random random,
            out int targetCount)
        {
            targetCount = 0;
            if (shape == null)
            {
                Debug.LogError("GalaxyNodePlacer.TryPlace requires a shape.");
                return false;
            }

            if (settings == null)
            {
                Debug.LogError("GalaxyNodePlacer.TryPlace requires generation settings.");
                return false;
            }

            if (random == null)
            {
                Debug.LogError("GalaxyNodePlacer.TryPlace requires a random source.");
                return false;
            }

            if (shape.SectorCount < 1)
            {
                Debug.LogError("GalaxyNodePlacer.TryPlace requires SectorCount >= 1.");
                return false;
            }

            shape.Prepare(random);
            return TryRollNodeCount(shape, settings, random, out targetCount);
        }

        /// <summary>
        /// Samples rounded positions, tags each with <see cref="GalaxyShapeSO.Describe"/>, and
        /// picks a uniform home index. Does not enforce spacing or sector floors.
        /// </summary>
        private static bool TryGenerate(
            GalaxyShapeSO shape,
            GenerationSettingsSO settings,
            System.Random random,
            int targetCount,
            out GeneratedGalaxyLayout layout)
        {
            layout = null;
            var positions = new List<Vector2>(targetCount);
            for (var placed = 0; placed < targetCount; placed++)
            {
                if (!BestCandidateSampler.TrySample(
                        shape,
                        positions,
                        settings.CandidatesPerNode,
                        settings.MinSpacing,
                        random,
                        out var sample))
                {
                    return false;
                }

                // Round after spacing checks; re-check so quantization cannot pack nodes too close.
                var rounded = Round(sample);
                if (!ClearsExisting(rounded, positions, settings.MinSpacing))
                {
                    return false;
                }

                positions.Add(rounded);
            }

            var nodes = new GeneratedGalaxyNode[positions.Count];
            for (var i = 0; i < positions.Count; i++)
            {
                nodes[i] = new GeneratedGalaxyNode(positions[i], shape.Describe(positions[i]));
            }

            var homeIndex = shape.PickHomeIndex(nodes, random);
            if (homeIndex < 0 || homeIndex >= nodes.Length)
            {
                Debug.LogWarning(
                    $"GalaxyNodePlacer.TryPlace got invalid home index {homeIndex}; falling back to 0.");
                homeIndex = 0;
            }

            layout = new GeneratedGalaxyLayout(nodes, homeIndex);
            return true;
        }

        /// <summary>
        /// Rejects layouts that break min spacing or leave any sector under-populated.
        /// </summary>
        private static bool TryValidate(
            GeneratedGalaxyLayout layout,
            GalaxyShapeSO shape,
            GenerationSettingsSO settings)
        {
            if (layout == null || shape == null || settings == null)
            {
                return false;
            }

            if (!RespectsMinSpacing(layout.Nodes, settings.MinSpacing))
            {
                return false;
            }

            return MeetsSectorMinimum(layout.Nodes, shape.SectorCount);
        }

        /// <summary>
        /// Rolls an inclusive count in TargetNodeCount ± variance, raised if needed so every
        /// sector can still hold <see cref="kMinNodesPerSector"/> nodes. Returns false when the
        /// authored settings cannot meet that sector floor even at the top of the variance band.
        /// </summary>
        private static bool TryRollNodeCount(
            GalaxyShapeSO shape,
            GenerationSettingsSO settings,
            System.Random random,
            out int nodeCount)
        {
            nodeCount = 0;
            var target = Mathf.Max(2, settings.TargetNodeCount);
            var varianceFraction = Mathf.Clamp01(settings.NodeCountVariancePercentage / 100f);
            var varianceMin = Mathf.Max(2, Mathf.FloorToInt(target * (1f - varianceFraction)));
            var varianceMax = Mathf.Max(varianceMin, Mathf.CeilToInt(target * (1f + varianceFraction)));
            var sectorFloor = shape.SectorCount * kMinNodesPerSector;
            var minCount = Mathf.Max(varianceMin, sectorFloor);
            if (minCount > varianceMax)
            {
                Debug.LogError(
                    "GalaxyNodePlacer.TryPlace: TargetNodeCount and variance cannot satisfy "
                    + $"{kMinNodesPerSector} nodes per sector ({shape.SectorCount} sectors).");
                return false;
            }

            nodeCount = random.Next(minCount, varianceMax + 1);
            return true;
        }

        /// <summary>
        /// True when every sector index in 0..<paramref name="sectorCount"/> has at least
        /// <see cref="kMinNodesPerSector"/> nodes. Out-of-range sector tags fail the layout.
        /// </summary>
        private static bool MeetsSectorMinimum(
            IReadOnlyList<GeneratedGalaxyNode> nodes,
            int sectorCount)
        {
            var counts = new int[sectorCount];
            for (var i = 0; i < nodes.Count; i++)
            {
                var sector = nodes[i].Tags.SectorIndex;
                if (sector < 0 || sector >= sectorCount)
                {
                    Debug.LogWarning(
                        $"GalaxyNodePlacer.TryPlace got sector index {sector} outside 0..{sectorCount - 1}.");
                    return false;
                }

                counts[sector]++;
            }

            for (var sector = 0; sector < sectorCount; sector++)
            {
                if (counts[sector] < kMinNodesPerSector)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector2 Round(Vector2 point)
        {
            return new Vector2(
                Mathf.Round(point.x / kPositionQuantum) * kPositionQuantum,
                Mathf.Round(point.y / kPositionQuantum) * kPositionQuantum);
        }

        private static bool RespectsMinSpacing(
            IReadOnlyList<GeneratedGalaxyNode> nodes,
            float minSpacing)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                for (var j = i + 1; j < nodes.Count; j++)
                {
                    if (Vector2.Distance(nodes[i].Position, nodes[j].Position) < minSpacing)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ClearsExisting(
            Vector2 point,
            IReadOnlyList<Vector2> existing,
            float minSpacing)
        {
            var spacing = Mathf.Max(0f, minSpacing);
            for (var i = 0; i < existing.Count; i++)
            {
                if (Vector2.Distance(point, existing[i]) < spacing)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
