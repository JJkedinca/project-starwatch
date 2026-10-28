using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Open disc galaxy in the unit box: sectors as angular wedges around the disc center.
    /// Density is 1 at the center and falls to <see cref="EdgeFalloff"/> at the rim.
    /// Home is a random placed node in the inner Progress band (near center, not on it).
    /// </summary>
    [CreateAssetMenu(fileName = "CircleGalaxyShape", menuName = "Starwatch/Map/Generation/Shapes/Circle")]
    public sealed class CircleGalaxyShapeSO : GalaxyShapeSO
    {
        /// <summary>Disc radius in unit-box space so the circle fits inside [0,1]×[0,1] around the center.</summary>
        private const float kUnitRadius = 0.5f;

        private const float kTwoPi = Mathf.PI * 2f;

        [Tooltip("Number of angular wedges used as sectors.")]
        [SerializeField]
        private int _wedgeCount = 4;

        [Tooltip("Density at the rim. Center density is 1; values fade toward this at the edge.")]
        [SerializeField]
        private float _edgeFalloff = 0.6f;

        [Tooltip("Minimum normalized radius for home. Keeps home off the exact disc center.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _homeMinProgress = 0.08f;

        [Tooltip("Maximum normalized radius for home. Keeps home in the inner disc, not the rim.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _homeMaxProgress = 0.35f;

        /// <summary>Seeded rotation applied to wedge boundaries so sector layout varies per prepare.</summary>
        private float _sectorRotation;

        /// <summary>Geometric center of the disc (home is a nearby placed node, not this point).</summary>
        public static Vector2 DiscCenter => new(0.5f, 0.5f);

        public override int SectorCount => WedgeCount;

        public int WedgeCount => Mathf.Max(1, _wedgeCount);

        public float EdgeFalloff => Mathf.Clamp01(_edgeFalloff);

        public float HomeMinProgress => Mathf.Clamp01(Mathf.Min(_homeMinProgress, _homeMaxProgress));

        public float HomeMaxProgress => Mathf.Clamp01(Mathf.Max(_homeMinProgress, _homeMaxProgress));

        /// <summary>
        /// Picks a random wedge rotation. Must run before placement or <see cref="Describe"/> is stale.
        /// </summary>
        public override void Prepare(System.Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            _sectorRotation = (float)(random.NextDouble() * kTwoPi);
        }

        /// <summary>
        /// Radial falloff inside the disc; zero outside. Sampler uses this as an accept probability.
        /// </summary>
        public override float Density(Vector2 point)
        {
            var normalizedRadius = NormalizedRadius(point);
            if (normalizedRadius > 1f)
            {
                return 0f;
            }

            return Mathf.Lerp(1f, EdgeFalloff, normalizedRadius);
        }

        /// <summary>
        /// Maps a point to a wedge index after <see cref="_sectorRotation"/>.
        /// Progress is normalized distance from the disc center (0 center → 1 rim). PathIndex matches sector.
        /// </summary>
        public override NodeShapeTags Describe(Vector2 point)
        {
            var offset = point - DiscCenter;
            var angle = Mathf.Atan2(offset.y, offset.x);

            // Rotate into wedge space and wrap into [0, 2π).
            var rotated = angle - _sectorRotation;
            rotated %= kTwoPi;
            if (rotated < 0f)
            {
                rotated += kTwoPi;
            }

            var wedgeSize = kTwoPi / WedgeCount;
            var sector = Mathf.Min(WedgeCount - 1, (int)(rotated / wedgeSize));
            var progress = Mathf.Clamp01(NormalizedRadius(point));
            return new NodeShapeTags(sector, sector, progress, isCore: false);
        }

        /// <summary>
        /// Prefers a random node whose Progress sits in the inner home band. If none qualify,
        /// picks the node whose Progress is closest to the band midpoint.
        /// </summary>
        public override int PickHomeIndex(IReadOnlyList<GeneratedGalaxyNode> nodes, System.Random random)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            if (random == null)
            {
                return 0;
            }

            var minProgress = HomeMinProgress;
            var maxProgress = HomeMaxProgress;
            var candidates = new List<int>();
            for (var i = 0; i < nodes.Count; i++)
            {
                var progress = nodes[i].Tags.Progress;
                if (progress >= minProgress && progress <= maxProgress)
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[random.Next(0, candidates.Count)];
            }

            var target = (minProgress + maxProgress) * 0.5f;
            var bestIndex = 0;
            var bestDistance = Mathf.Abs(nodes[0].Tags.Progress - target);
            for (var i = 1; i < nodes.Count; i++)
            {
                var distance = Mathf.Abs(nodes[i].Tags.Progress - target);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>0 at disc center, 1 at the rim, greater than 1 outside the disc.</summary>
        private float NormalizedRadius(Vector2 point)
        {
            return (point - DiscCenter).magnitude / kUnitRadius;
        }
    }
}
