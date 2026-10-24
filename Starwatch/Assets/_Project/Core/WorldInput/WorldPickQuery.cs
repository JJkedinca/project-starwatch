using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Parameters for a world-space pointer pick.
    /// </summary>
    public readonly struct WorldPickQuery
    {
        public LayerMask Mask { get; }

        public float TolerancePixels { get; }

        public float MaxDistance { get; }

        public WorldPickQuery(LayerMask mask, float tolerancePixels, float maxDistance)
        {
            Mask = mask;
            TolerancePixels = tolerancePixels;
            MaxDistance = maxDistance;
        }
    }
}
