using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Physics layers authored in TagManager. Values must match Project Settings.
    /// </summary>
    public enum StarwatchLayers
    {
        MapNode = 6,
        MapFleet = 7,
    }

    /// <summary>
    /// Helpers for building layer masks from <see cref="StarwatchLayers"/>.
    /// </summary>
    public static class StarwatchLayersExtensions
    {
        public static int ToIndex(this StarwatchLayers layer)
        {
            return (int)layer;
        }

        public static LayerMask ToMask(this StarwatchLayers layer)
        {
            return 1 << (int)layer;
        }

        public static LayerMask ToMask(params StarwatchLayers[] layers)
        {
            var mask = 0;
            if (layers == null)
            {
                return mask;
            }

            for (var i = 0; i < layers.Length; i++)
            {
                mask |= 1 << (int)layers[i];
            }

            return mask;
        }
    }
}
