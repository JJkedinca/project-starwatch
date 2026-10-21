using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
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

        /// <summary>
        /// Gap between the map-node edge and the fleet orbit ring.
        /// </summary>
        internal const float kFleetOrbitOutset = 8f;

        internal const float kFleetPipSize = 8f;

        internal const float kRoutePad = 4f;

        internal const float kRouteLineWidth = 3f;

        internal const float kRouteKnownLineWidth = 2f;

        internal const float kRoutePlannedLineWidth = 3f;

        internal const float kRouteTransitLineWidth = 3.5f;

        internal const float kRouteDashLength = 8f;

        internal const float kRouteDashGap = 6f;

        /// <summary>Dim cyan for known routes.</summary>
        internal static readonly Color kRouteKnownColor = new Color(0.35f, 0.72f, 0.82f, 0.55f);

        /// <summary>Gold for the planned hover course.</summary>
        internal static readonly Color kRoutePlannedColor = new Color(0.90f, 0.75f, 0.28f, 0.95f);

        /// <summary>Orange for fleets in transit.</summary>
        internal static readonly Color kRouteTransitColor = new Color(0.95f, 0.55f, 0.20f, 0.95f);

        internal static readonly Color kRouteColor = kRouteKnownColor;

        /// <summary>
        /// Padding from the map-surface top-left to the first fitted node edge.
        /// </summary>
        internal static readonly Vector2 kDefaultPadding = new Vector2(48f, 48f);

        /// <summary>
        /// Maps a graph position into a top-left style absolute UI position.
        /// Y is flipped so authored +Y reads upward on the map surface.
        /// </summary>
        internal static Vector2 ToLocalPosition(Vector2 mapPosition, float scale, Vector2 originOffset)
        {
            return new Vector2(originOffset.x + (mapPosition.x * scale), originOffset.y + (-mapPosition.y * scale));
        }

        /// <summary>
        /// Builds an origin so the highest authored Y sits at <paramref name="padding"/>.y
        /// and the leftmost authored X sits at <paramref name="padding"/>.x.
        /// </summary>
        internal static Vector2 ComputeOriginOffset(IReadOnlyList<Vector2> mapPositions, float scale, Vector2 padding)
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

            return new Vector2(padding.x - (minX * scale), padding.y + (maxY * scale));
        }

        /// <summary>
        /// Returns the center of a map-node element for route endpoints.
        /// </summary>
        internal static Vector2 ToNodeCenter(Vector2 mapPosition, float scale, Vector2 originOffset)
        {
            var topLeft = ToLocalPosition(mapPosition, scale, originOffset);
            var half = kNodeSize * 0.5f;
            return new Vector2(topLeft.x + half, topLeft.y + half);
        }
    }
}
