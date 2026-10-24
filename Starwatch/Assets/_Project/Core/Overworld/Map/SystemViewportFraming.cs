using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Frames a world point in the uncovered portion of the screen (left of right-column panels).
    /// </summary>
    internal static class SystemViewportFraming
    {
        /// <summary>
        /// Camera XZ focus so <paramref name="starWorld"/> sits at the center of the uncovered viewport.
        /// With a top-down camera, shifting the camera opposite the panel inset recenters content.
        /// </summary>
        internal static Vector3 CameraFocusForUncoveredCenter(
            Camera camera,
            Vector3 starWorld,
            float cameraHeight,
            float rightPanelInsetNormalized)
        {
            if (camera == null)
            {
                return starWorld;
            }

            var inset = Mathf.Clamp01(rightPanelInsetNormalized);
            if (inset <= 0.0001f)
            {
                return new Vector3(starWorld.x, cameraHeight, starWorld.z);
            }

            // Visible region is [0, 1-inset] of screen width; its center is at (1-inset)/2 from the left.
            // Screen geometric center is 0.5. Offset in normalized screen X: 0.5 - (1-inset)/2 = inset/2.
            var rect = camera.pixelRect;
            var screenOffsetX = rect.width * (inset * 0.5f);
            var worldShift = MapPanMath.ScreenDeltaToWorldDelta(
                camera,
                new Vector2(screenOffsetX, 0f),
                cameraHeight);
            // ScreenDeltaToWorldDelta pans camera opposite drag; we want the star to appear left of center,
            // so move the camera right (positive world shift after the negation in ScreenDelta).
            var focus = starWorld - worldShift;
            return new Vector3(focus.x, cameraHeight, focus.z);
        }
    }
}
