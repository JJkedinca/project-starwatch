using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Physics-based world pick: exact ray first, then a pixel-tolerance sphere cast.
    /// </summary>
    public static class WorldPicker
    {
        public static bool TryPick(
            Camera camera,
            Vector2 screen,
            in WorldPickQuery query,
            out RaycastHit hit)
        {
            hit = default;
            if (camera == null)
            {
                return false;
            }

            var ray = camera.ScreenPointToRay(screen);
            var maxDistance = query.MaxDistance > 0f ? query.MaxDistance : camera.farClipPlane;
            if (Physics.Raycast(
                    ray,
                    out hit,
                    maxDistance,
                    query.Mask,
                    QueryTriggerInteraction.Collide))
            {
                return true;
            }

            if (query.TolerancePixels <= 0f)
            {
                return false;
            }

            var distanceToPlane = Mathf.Abs(ray.origin.y / ray.direction.y);
            if (distanceToPlane < 0.0001f || float.IsInfinity(distanceToPlane) || float.IsNaN(distanceToPlane))
            {
                distanceToPlane = Mathf.Abs(camera.transform.position.y);
            }

            var worldHeight = 2f
                * distanceToPlane
                * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var pixelHeight = Mathf.Max(1f, camera.pixelRect.height);
            var radius = query.TolerancePixels * (worldHeight / pixelHeight);
            return Physics.SphereCast(
                ray,
                radius,
                out hit,
                maxDistance,
                query.Mask,
                QueryTriggerInteraction.Collide);
        }
    }
}
