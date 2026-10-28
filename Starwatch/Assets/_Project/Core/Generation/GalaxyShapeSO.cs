using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Geometry-only galaxy shape in unit-box space [0,1]×[0,1]. Shared placement calls into this;
    /// subclasses configure density, sector labels, and home selection after nodes are placed.
    /// </summary>
    public abstract class GalaxyShapeSO : ScriptableObject
    {
        /// <summary>
        /// Number of sectors this shape labels (wedges, arms, or lanes). Placement requires
        /// at least three nodes in each.
        /// </summary>
        public abstract int SectorCount { get; }


        /// <summary>
        /// Minimum routes from each ring into the next. Used after rings exist.
        /// </summary>
        public virtual int MinOutwardRoutes => 3;

        /// <summary>
        /// Rolls any shape-local randomness (sector rotation, arm offsets, etc.). Call before placement.
        /// </summary>
        public virtual void Prepare(System.Random random)
        {
        }

        /// <summary>
        /// Density in [0, 1] used as an accept probability when sampling the unit box.
        /// Zero forbids a node; higher values attract more samples (arms, lanes, disc interior).
        /// </summary>
        public abstract float Density(Vector2 point);

        /// <summary>
        /// Sector and path tags for a placed point. Call only after <see cref="Prepare"/>.
        /// </summary>
        public abstract NodeShapeTags Describe(Vector2 point);

        /// <summary>
        /// Picks the home node index after placement. Default is uniform among all nodes.
        /// </summary>
        public virtual int PickHomeIndex(IReadOnlyList<GeneratedGalaxyNode> nodes, System.Random random)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            if (random == null)
            {
                return 0;
            }

            return random.Next(0, nodes.Count);
        }

        /// <summary>
        /// Cost multiplier for a candidate route. <see cref="float.PositiveInfinity"/> forbids the edge.
        /// </summary>
        public virtual float RouteCost(Vector2 a, Vector2 b)
        {
            return 1f;
        }

        /// <summary>
        /// Optional route group with a keep-cap (for example arm-pair bridges).
        /// </summary>
        public virtual bool TryGetRouteGroup(Vector2 a, Vector2 b, out int groupId, out int maxRoutes)
        {
            groupId = 0;
            maxRoutes = 0;
            return false;
        }

        /// <summary>
        /// Override when density sampling cannot place nodes (for example Clusters).
        /// Default uses best-candidate sampling via <see cref="GalaxyNodePlacer"/>.
        /// </summary>
        public virtual bool TryPlaceNodes(
            GenerationSettingsSO settings,
            System.Random random,
            out GeneratedGalaxyLayout layout)
        {
            return GalaxyNodePlacer.TryPlace(this, settings, random, out layout);
        }
    }
}
