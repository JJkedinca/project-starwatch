using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// How a map route segment is drawn.
    /// </summary>
    internal enum MapRouteState
    {
        Known = 0,
        Planned = 1,
        InTransit = 2,
    }

    /// <summary>
    /// One visible route segment in map space.
    /// </summary>
    internal readonly struct MapRouteSegment
    {
        public ContentId NodeAId { get; }

        public ContentId NodeBId { get; }

        public Vector2 MapPositionA { get; }

        public Vector2 MapPositionB { get; }

        public MapRouteState State { get; }

        public MapRouteSegment(
            ContentId nodeAId,
            ContentId nodeBId,
            Vector2 mapPositionA,
            Vector2 mapPositionB,
            MapRouteState state)
        {
            NodeAId = nodeAId;
            NodeBId = nodeBId;
            MapPositionA = mapPositionA;
            MapPositionB = mapPositionB;
            State = state;
        }
    }
}
