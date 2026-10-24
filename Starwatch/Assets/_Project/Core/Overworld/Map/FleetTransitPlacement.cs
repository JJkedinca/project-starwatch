using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Places a transit fleet marker along a map-space route using sim progress.
    /// </summary>
    internal static class FleetTransitPlacement
    {
        /// <summary>
        /// Progress is 0 at departure and 1 at arrival:
        /// (<paramref name="totalSeconds"/> − <paramref name="remainingSeconds"/>) / total.
        /// </summary>
        internal static float Progress(float remainingSeconds, float totalSeconds)
        {
            if (totalSeconds <= 0.0001f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - (remainingSeconds / totalSeconds));
        }

        internal static Vector3 Evaluate(Vector3 fromWorld, Vector3 toWorld, float progress)
        {
            return Vector3.Lerp(fromWorld, toWorld, Mathf.Clamp01(progress));
        }

        internal static Vector3 Evaluate(
            Vector3 fromWorld,
            Vector3 toWorld,
            float remainingSeconds,
            float totalSeconds)
        {
            return Evaluate(fromWorld, toWorld, Progress(remainingSeconds, totalSeconds));
        }

        /// <summary>
        /// Yaw in degrees so a forward (+Z) marker points along the route on the map plane.
        /// </summary>
        internal static float YawDegrees(Vector3 fromWorld, Vector3 toWorld)
        {
            var delta = toWorld - fromWorld;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            return Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
        }
    }
}
