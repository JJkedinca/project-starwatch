using System.Collections.Generic;
using NUnit.Framework;
using ProjectStarwatch.Core.Generation;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.Generation
{
    /// <summary>
    /// EditMode coverage for circle galaxy density placement.
    /// </summary>
    public sealed class CircleGalaxyPlacementTests
    {
        private const int kWedgeCount = 4;
        private const int kMinNodesPerSector = 3;

        [Test]
        public void TryPlace_PicksValidHome_CountInVariance_AndRespectsSpacing()
        {
            var shape = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            var settings = ScriptableObject.CreateInstance<GenerationSettingsSO>();
            Assert.That(TryPlaceWithRetry(shape, settings, seed: 42, out var layout), Is.True);

            GetNodeCountRange(shape, settings, out var minCount, out var maxCount);
            Assert.That(layout.Nodes.Count, Is.InRange(minCount, maxCount));
            Assert.That(layout.HomeIndex, Is.InRange(0, layout.Nodes.Count - 1));
            Assert.That(RespectsMinSpacing(layout, settings.MinSpacing), Is.True);

            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void TryPlace_HomeIsInInnerProgressBand()
        {
            var shape = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            var settings = ScriptableObject.CreateInstance<GenerationSettingsSO>();
            Assert.That(TryPlaceWithRetry(shape, settings, seed: 42, out var layout), Is.True);

            var homeProgress = layout.Nodes[layout.HomeIndex].Tags.Progress;
            var hasBandCandidate = false;
            for (var i = 0; i < layout.Nodes.Count; i++)
            {
                var progress = layout.Nodes[i].Tags.Progress;
                if (progress >= shape.HomeMinProgress && progress <= shape.HomeMaxProgress)
                {
                    hasBandCandidate = true;
                    break;
                }
            }

            if (hasBandCandidate)
            {
                Assert.That(homeProgress, Is.GreaterThanOrEqualTo(shape.HomeMinProgress));
                Assert.That(homeProgress, Is.LessThanOrEqualTo(shape.HomeMaxProgress));
            }
            else
            {
                Assert.That(homeProgress, Is.LessThan(0.5f));
            }

            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void TryPlace_SameRandomSeed_ProducesSameLayout()
        {
            var shapeA = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            var shapeB = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            var settingsA = ScriptableObject.CreateInstance<GenerationSettingsSO>();
            var settingsB = ScriptableObject.CreateInstance<GenerationSettingsSO>();

            Assert.That(
                TryFindSuccessfulSeed(shapeA, settingsA, startSeed: 99, out var workingSeed, out var first),
                Is.True);
            Assert.That(shapeB.TryPlaceNodes(settingsB, new System.Random(workingSeed), out var second), Is.True);

            Assert.That(first.Nodes.Count, Is.EqualTo(second.Nodes.Count));
            Assert.That(first.HomeIndex, Is.EqualTo(second.HomeIndex));
            for (var i = 0; i < first.Nodes.Count; i++)
            {
                Assert.That(first.Nodes[i].Position, Is.EqualTo(second.Nodes[i].Position));
                Assert.That(first.Nodes[i].Tags.SectorIndex, Is.EqualTo(second.Nodes[i].Tags.SectorIndex));
            }

            Object.DestroyImmediate(shapeA);
            Object.DestroyImmediate(shapeB);
            Object.DestroyImmediate(settingsA);
            Object.DestroyImmediate(settingsB);
        }

        [Test]
        public void TryPlace_EnforcesAtLeastThreeNodesPerSector()
        {
            var shape = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();
            var settings = ScriptableObject.CreateInstance<GenerationSettingsSO>();
            Assert.That(TryPlaceWithRetry(shape, settings, seed: 7, out var layout), Is.True);

            var counts = new int[kWedgeCount];
            for (var i = 0; i < layout.Nodes.Count; i++)
            {
                counts[layout.Nodes[i].Tags.SectorIndex]++;
            }

            for (var sector = 0; sector < kWedgeCount; sector++)
            {
                Assert.That(counts[sector], Is.GreaterThanOrEqualTo(kMinNodesPerSector));
            }

            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Density_IsZeroOutsideDisc_AndPositiveInside()
        {
            var shape = ScriptableObject.CreateInstance<CircleGalaxyShapeSO>();

            Assert.That(shape.Density(CircleGalaxyShapeSO.DiscCenter), Is.EqualTo(1f).Within(0.001f));
            Assert.That(shape.Density(new Vector2(0f, 0f)), Is.EqualTo(0f));
            Assert.That(shape.Density(CircleGalaxyShapeSO.DiscCenter + new Vector2(0.25f, 0f)), Is.GreaterThan(0f));

            Object.DestroyImmediate(shape);
        }

        private static void GetNodeCountRange(
            GalaxyShapeSO shape,
            GenerationSettingsSO settings,
            out int minCount,
            out int maxCount)
        {
            var target = Mathf.Max(2, settings.TargetNodeCount);
            var varianceFraction = Mathf.Clamp01(settings.NodeCountVariancePercentage / 100f);
            var varianceMin = Mathf.Max(2, Mathf.FloorToInt(target * (1f - varianceFraction)));
            var varianceMax = Mathf.Max(varianceMin, Mathf.CeilToInt(target * (1f + varianceFraction)));
            minCount = Mathf.Max(varianceMin, shape.SectorCount * kMinNodesPerSector);
            maxCount = Mathf.Max(minCount, varianceMax);
        }

        private static bool TryPlaceWithRetry(
            CircleGalaxyShapeSO shape,
            GenerationSettingsSO settings,
            int seed,
            out GeneratedGalaxyLayout layout)
        {
            return TryFindSuccessfulSeed(shape, settings, seed, out _, out layout);
        }

        private static bool TryFindSuccessfulSeed(
            CircleGalaxyShapeSO shape,
            GenerationSettingsSO settings,
            int startSeed,
            out int workingSeed,
            out GeneratedGalaxyLayout layout)
        {
            for (var attempt = 0; attempt < 64; attempt++)
            {
                workingSeed = startSeed + attempt;
                if (shape.TryPlaceNodes(settings, new System.Random(workingSeed), out layout))
                {
                    return true;
                }
            }

            workingSeed = -1;
            layout = null;
            return false;
        }

        private static bool RespectsMinSpacing(GeneratedGalaxyLayout layout, float minSpacing)
        {
            for (var i = 0; i < layout.Nodes.Count; i++)
            {
                for (var j = i + 1; j < layout.Nodes.Count; j++)
                {
                    if (Vector2.Distance(layout.Nodes[i].Position, layout.Nodes[j].Position) < minSpacing)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
