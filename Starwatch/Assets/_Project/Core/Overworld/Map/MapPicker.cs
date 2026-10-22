using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Picks the nearest map node by intersecting a ray with the map plane.
    /// </summary>
    internal static class MapPicker
    {
        internal const float kDefaultPickRadius = 2.5f;

        private static readonly Plane MapPlane = new(Vector3.up, Vector3.zero);

        internal readonly struct NodePickPoint
        {
            public ContentId NodeId { get; }

            public Vector3 WorldPosition { get; }

            public NodePickPoint(ContentId nodeId, Vector3 worldPosition)
            {
                NodeId = nodeId;
                WorldPosition = worldPosition;
            }
        }

        internal static bool TryIntersectMapPlane(Ray ray, out Vector3 hitPoint)
        {
            if (MapPlane.Raycast(ray, out var distance))
            {
                hitPoint = ray.GetPoint(distance);
                return true;
            }

            hitPoint = default;
            return false;
        }

        /// <summary>
        /// Returns the nearest node within <paramref name="pickRadius"/> of the plane hit.
        /// Ties break by lower ContentId string (stable).
        /// </summary>
        internal static bool TryPickNearest(
            Ray ray,
            IReadOnlyList<NodePickPoint> nodes,
            float pickRadius,
            out ContentId nodeId)
        {
            nodeId = default;
            if (nodes == null || nodes.Count == 0 || pickRadius < 0f)
            {
                return false;
            }

            if (!TryIntersectMapPlane(ray, out var hit))
            {
                return false;
            }

            var bestDistanceSq = pickRadius * pickRadius;
            var found = false;
            ContentId bestId = default;
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.NodeId.IsEmpty)
                {
                    continue;
                }

                var delta = node.WorldPosition - hit;
                delta.y = 0f;
                var distanceSq = delta.sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                {
                    continue;
                }

                if (!found
                    || distanceSq < bestDistanceSq - 0.000001f
                    || (Mathf.Abs(distanceSq - bestDistanceSq) <= 0.000001f
                        && string.CompareOrdinal(node.NodeId.Value, bestId.Value) < 0))
                {
                    found = true;
                    bestDistanceSq = distanceSq;
                    bestId = node.NodeId;
                }
            }

            if (!found)
            {
                return false;
            }

            nodeId = bestId;
            return true;
        }
    }
}
