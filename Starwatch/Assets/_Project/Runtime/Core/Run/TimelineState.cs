using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Mutable empire state for the current timeline attempt. Resets on rewind.
    /// Owns its collections; must not alias scenario or archive lists.
    /// Home system lives on Scenario.StartingEmpire — not duplicated here.
    /// </summary>
    public sealed class TimelineState
    {
        public EntityIdFactory EntityIds { get; }

        public IReadOnlyList<FleetPlaceholderState> Fleets => _fleets;

        private readonly List<FleetPlaceholderState> _fleets = new List<FleetPlaceholderState>();

        public TimelineState(EntityIdFactory entityIds)
        {
            EntityIds = entityIds ?? new EntityIdFactory();
        }

        /// <summary>
        /// Adds a placeholder fleet when class, location, and ship count are valid.
        /// </summary>
        public bool TryAddFleet(
            ContentId classId,
            int shipCount,
            ContentId locationSystemId,
            out EntityId id)
        {
            id = default;
            if (classId.IsEmpty || locationSystemId.IsEmpty || shipCount <= 0)
            {
                return false;
            }

            id = EntityIds.Next();
            _fleets.Add(new FleetPlaceholderState(id, classId, shipCount, locationSystemId));
            return true;
        }
    }
}
