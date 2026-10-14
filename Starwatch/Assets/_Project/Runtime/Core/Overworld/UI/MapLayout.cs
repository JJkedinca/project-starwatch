using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Converts authored map coordinates into UI Toolkit local positions.
    /// </summary>
    internal static class MapLayout
    {
        internal const float kDefaultScale = 72f;

        /// <summary>
        /// Map-node width and height. Applied on the element so route centers
        /// stay aligned if USS visuals change.
        /// </summary>
        internal const float kNodeSize = 56f;

        internal const float kRoutePad = 4f;

        internal const float kRouteLineWidth = 3f;

        internal static readonly Color kRouteColor = new Color(0.43f, 0.59f, 0.75f, 0.9f);

        /// <summary>
        /// Padding from the map-surface top-left to the first fitted node edge.
        /// </summary>
        internal static readonly Vector2 kDefaultPadding = new Vector2(48f, 48f);

        /// <summary>
        /// Maps a graph position into a top-left style absolute UI position.
        /// Y is flipped so authored +Y reads upward on the map surface.
        /// </summary>
        internal static Vector2 ToLocalPosition(
            Vector2 mapPosition,
            float scale,
            Vector2 originOffset)
        {
            return new Vector2(
                originOffset.x + (mapPosition.x * scale),
                originOffset.y + (-mapPosition.y * scale));
        }

        /// <summary>
        /// Builds an origin so the highest authored Y sits at <paramref name="padding"/>.y
        /// and the leftmost authored X sits at <paramref name="padding"/>.x.
        /// </summary>
        internal static Vector2 ComputeOriginOffset(
            IReadOnlyList<Vector2> mapPositions,
            float scale,
            Vector2 padding)
        {
            if (mapPositions == null || mapPositions.Count == 0)
            {
                return padding;
            }

            var minX = mapPositions[0].x;
            var maxY = mapPositions[0].y;
            for (var i = 1; i < mapPositions.Count; i++)
            {
                var position = mapPositions[i];
                if (position.x < minX)
                {
                    minX = position.x;
                }

                if (position.y > maxY)
                {
                    maxY = position.y;
                }
            }

            return new Vector2(
                padding.x - (minX * scale),
                padding.y + (maxY * scale));
        }

        /// <summary>
        /// Returns the center of a map-node element for route endpoints.
        /// </summary>
        internal static Vector2 ToNodeCenter(
            Vector2 mapPosition,
            float scale,
            Vector2 originOffset)
        {
            var topLeft = ToLocalPosition(mapPosition, scale, originOffset);
            var half = kNodeSize * 0.5f;
            return new Vector2(topLeft.x + half, topLeft.y + half);
        }
    }
}
