using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// One ship waiting or building at a colony. The head order is the one whose time advances.
    /// </summary>
    public sealed class ShipBuildOrder
    {
        public ContentId ClassId { get; }

        public float SecondsRemaining { get; private set; }

        public ShipBuildOrder(ContentId classId, float secondsRemaining)
        {
            ClassId = classId;
            SecondsRemaining = secondsRemaining < 0f ? 0f : secondsRemaining;
        }

        /// <summary>
        /// Applies simulation time. Returns the seconds left after this order finishes, or zero while it is still building.
        /// </summary>
        internal float Consume(float seconds)
        {
            if (seconds <= 0f)
            {
                return 0f;
            }

            if (seconds < SecondsRemaining)
            {
                SecondsRemaining -= seconds;
                return 0f;
            }

            var leftover = seconds - SecondsRemaining;
            SecondsRemaining = 0f;
            return leftover;
        }
    }
}
