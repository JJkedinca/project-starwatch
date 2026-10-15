using System;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Minimal timeline-owned fleet placeholder until the fleet slice lands.
    /// Immutable after construction; TimelineState validates before insert.
    /// TODO: Replace with full fleet/runtime ship state when Phase 4 fleets land.
    /// </summary>
    public sealed class FleetPlaceholderState
    {
        public EntityId Id { get; }

        public ContentId ClassId { get; }

        public int ShipCount { get; }

        public ContentId LocationSystemId { get; }

        public FleetPlaceholderState(EntityId id, ContentId classId, int shipCount, ContentId locationSystemId)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("Fleet id is required.", nameof(id));
            }

            if (classId.IsEmpty)
            {
                throw new ArgumentException("Fleet class id is required.", nameof(classId));
            }

            if (shipCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shipCount), shipCount, "Ship count must be positive.");
            }

            if (locationSystemId.IsEmpty)
            {
                throw new ArgumentException("Location system id is required.", nameof(locationSystemId));
            }

            Id = id;
            ClassId = classId;
            ShipCount = shipCount;
            LocationSystemId = locationSystemId;
        }
    }
}
