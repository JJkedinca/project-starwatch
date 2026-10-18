using System;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored undirected route between two map nodes.
    /// </summary>
    [Serializable]
    public sealed class MapRouteDefinition
    {
        public ContentId NodeAId { get; }

        public ContentId NodeBId { get; }

        public float TravelDurationSeconds { get; }

        public MapRouteDefinition(ContentId nodeAId, ContentId nodeBId, float travelDurationSeconds)
        {
            NodeAId = nodeAId;
            NodeBId = nodeBId;
            TravelDurationSeconds = travelDurationSeconds;
        }
    }
}
