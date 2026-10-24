using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Converts authored 2D map coordinates into world space on the map plane (y = 0).
    /// Height is presentation-only and never used for gameplay.
    /// </summary>
    internal static class MapSpace
    {
        internal const float kWorldUnitsPerMapUnit = 10f;

        internal static Vector3 ToWorld(Vector2 mapPosition)
        {
            return new Vector3(
                mapPosition.x * kWorldUnitsPerMapUnit,
                0f,
                mapPosition.y * kWorldUnitsPerMapUnit);
        }

        internal static Vector2 ToMap(Vector3 worldPosition)
        {
            return new Vector2(
                worldPosition.x / kWorldUnitsPerMapUnit,
                worldPosition.z / kWorldUnitsPerMapUnit);
        }
    }
}
