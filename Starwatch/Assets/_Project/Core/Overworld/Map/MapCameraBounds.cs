using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Axis-aligned camera clamp region from node world positions plus a margin.
    /// </summary>
    internal readonly struct MapCameraBounds
    {
        internal const float kDefaultMargin = 15f;

        public static MapCameraBounds Empty { get; } = new(isEmpty: true);

        public Vector2 Min { get; }

        public Vector2 Max { get; }

        public bool IsEmpty { get; }

        public MapCameraBounds(Vector2 min, Vector2 max)
        {
            if (min.x > max.x || min.y > max.y)
            {
                Min = default;
                Max = default;
                IsEmpty = true;
                return;
            }

            Min = min;
            Max = max;
            IsEmpty = false;
        }

        private MapCameraBounds(bool isEmpty)
        {
            Min = default;
            Max = default;
            IsEmpty = isEmpty;
        }

        internal static MapCameraBounds FromWorldPoints(IReadOnlyList<Vector3> worldPoints, float margin)
        {
            if (worldPoints == null || worldPoints.Count == 0)
            {
                return Empty;
            }

            var minX = worldPoints[0].x;
            var maxX = worldPoints[0].x;
            var minZ = worldPoints[0].z;
            var maxZ = worldPoints[0].z;
            for (var i = 1; i < worldPoints.Count; i++)
            {
                var point = worldPoints[i];
                if (point.x < minX)
                {
                    minX = point.x;
                }

                if (point.x > maxX)
                {
                    maxX = point.x;
                }

                if (point.z < minZ)
                {
                    minZ = point.z;
                }

                if (point.z > maxZ)
                {
                    maxZ = point.z;
                }
            }

            var pad = margin < 0f ? 0f : margin;
            return new MapCameraBounds(
                new Vector2(minX - pad, minZ - pad),
                new Vector2(maxX + pad, maxZ + pad));
        }

        /// <summary>
        /// Clamps a camera XZ position into the bounds. A map smaller than the view
        /// still clamps to the box (no expansion). Empty bounds leave the camera alone.
        /// </summary>
        internal Vector3 ClampCameraPosition(Vector3 cameraPosition)
        {
            if (IsEmpty)
            {
                return cameraPosition;
            }

            return new Vector3(
                Mathf.Clamp(cameraPosition.x, Min.x, Max.x),
                cameraPosition.y,
                Mathf.Clamp(cameraPosition.z, Min.y, Max.y));
        }
    }
}
