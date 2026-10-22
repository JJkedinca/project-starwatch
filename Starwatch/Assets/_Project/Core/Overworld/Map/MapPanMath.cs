using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Converts pointer motion into world-plane pan so the map stays under the cursor.
    /// </summary>
    internal static class MapPanMath
    {
        /// <summary>
        /// Pointer delta in screen pixels → world delta on the map plane at the camera height.
        /// Positive screen-x (right) pans the camera left so content follows the drag.
        /// </summary>
        internal static Vector3 ScreenDeltaToWorldDelta(
            Camera camera,
            Vector2 screenDelta,
            float cameraHeight)
        {
            if (camera == null)
            {
                return Vector3.zero;
            }

            var height = Mathf.Abs(cameraHeight);
            if (height < 0.0001f)
            {
                height = 0.0001f;
            }

            var center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, height);
            var offset = new Vector3(
                (Screen.width * 0.5f) + screenDelta.x,
                (Screen.height * 0.5f) + screenDelta.y,
                height);
            var worldCenter = camera.ScreenToWorldPoint(center);
            var worldOffset = camera.ScreenToWorldPoint(offset);
            var delta = worldOffset - worldCenter;
            return new Vector3(-delta.x, 0f, -delta.z);
        }

        /// <summary>
        /// Keyboard pan: screen-relative axes at a fixed world speed scaled by camera height.
        /// </summary>
        internal static Vector3 KeyboardPanDelta(
            Camera camera,
            Vector2 panInput,
            float cameraHeight,
            float screenRelativeSpeed,
            float deltaTime)
        {
            if (camera == null || panInput.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            var right = camera.transform.right;
            right.y = 0f;
            right.Normalize();
            var forward = camera.transform.up;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            var heightFactor = Mathf.Abs(cameraHeight) / 20f;
            if (heightFactor < 0.25f)
            {
                heightFactor = 0.25f;
            }

            var speed = screenRelativeSpeed * heightFactor;
            var move = ((right * panInput.x) + (forward * panInput.y)) * speed * deltaTime;
            return new Vector3(move.x, 0f, move.z);
        }
    }
}
