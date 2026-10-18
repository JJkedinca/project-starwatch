using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Authored map-space endpoints for one visible route segment.
    /// </summary>
    internal readonly struct MapRouteSegment
    {
        public Vector2 MapPositionA { get; }

        public Vector2 MapPositionB { get; }

        public MapRouteSegment(Vector2 mapPositionA, Vector2 mapPositionB)
        {
            MapPositionA = mapPositionA;
            MapPositionB = mapPositionB;
        }
    }
}
