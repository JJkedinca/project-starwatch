using System;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Universe
{
    /// <summary>
    /// Undirected travel edge between two map nodes with a simulation duration.
    /// </summary>
    public sealed class MapRoute
    {
        public ContentId NodeAId { get; }

        public ContentId NodeBId { get; }

        public float TravelDurationSeconds { get; }

        public MapRoute(ContentId nodeAId, ContentId nodeBId, float travelDurationSeconds)
        {
            if (travelDurationSeconds < 0f || float.IsNaN(travelDurationSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(travelDurationSeconds),
                    travelDurationSeconds,
                    "Travel duration must be non-negative.");
            }

            NodeAId = nodeAId;
            NodeBId = nodeBId;
            TravelDurationSeconds = travelDurationSeconds;
        }

        /// <summary>
        /// Returns whether this route connects the two node ids in either order.
        /// </summary>
        public bool Connects(ContentId left, ContentId right)
        {
            return (NodeAId == left && NodeBId == right)
                || (NodeAId == right && NodeBId == left);
        }

        /// <summary>
        /// Stable undirected key for archive route knowledge.
        /// </summary>
        public static ContentId MakeKnowledgeKey(ContentId left, ContentId right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return default;
            }

            var a = left.Value;
            var b = right.Value;
            if (string.CompareOrdinal(a, b) > 0)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            return ContentId.From(a + "|" + b);
        }

        /// <summary>
        /// Returns the opposite endpoint when the given id is on this route.
        /// </summary>
        public bool TryGetOther(ContentId nodeId, out ContentId otherId)
        {
            if (NodeAId == nodeId)
            {
                otherId = NodeBId;
                return true;
            }

            if (NodeBId == nodeId)
            {
                otherId = NodeAId;
                return true;
            }

            otherId = default;
            return false;
        }
    }
}
