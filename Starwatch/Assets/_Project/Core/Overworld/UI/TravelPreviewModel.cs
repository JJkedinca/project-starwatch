using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Whether the selected fleet can travel to a node, and how long it would take.
    /// Matches <see cref="Services.TravelService"/> rules for StartTravel.
    /// </summary>
    internal readonly struct TravelPreviewModel
    {
        public static TravelPreviewModel None { get; } = new(false, 0f, default, default);

        public bool Reachable { get; }

        public float Seconds { get; }

        public ContentId FromNodeId { get; }

        public ContentId ToNodeId { get; }

        public TravelPreviewModel(
            bool reachable,
            float seconds,
            ContentId fromNodeId,
            ContentId toNodeId)
        {
            Reachable = reachable;
            Seconds = seconds < 0f ? 0f : seconds;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
        }
    }
}
