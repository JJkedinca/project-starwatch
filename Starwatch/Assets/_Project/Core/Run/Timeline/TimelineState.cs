using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Mutable empire state for the current timeline attempt. Resets on rewind.
    /// Owns its collections; must not alias universe or archive collections.
    /// Home system lives on Universe.StartingEmpire — not duplicated here.
    /// </summary>
    public sealed class TimelineState
    {
        public EntityIdFactory EntityIds { get; }

        public IReadOnlyList<FleetPlaceholderState> Fleets => _fleets;

        public IReadOnlyList<ActiveTravel> ActiveTravels => _activeTravels;

        private readonly List<FleetPlaceholderState> _fleets = new();
        private readonly List<ActiveTravel> _activeTravels = new();

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

        /// <summary>
        /// Looks up a fleet by id.
        /// </summary>
        public bool TryGetFleet(EntityId fleetId, out FleetPlaceholderState fleet)
        {
            for (var i = 0; i < _fleets.Count; i++)
            {
                if (_fleets[i].Id == fleetId)
                {
                    fleet = _fleets[i];
                    return true;
                }
            }

            fleet = null;
            return false;
        }

        /// <summary>
        /// Replaces a fleet's location with a validated new immutable instance.
        /// </summary>
        public bool TrySetFleetLocation(EntityId fleetId, ContentId locationSystemId)
        {
            if (locationSystemId.IsEmpty)
            {
                return false;
            }

            for (var i = 0; i < _fleets.Count; i++)
            {
                var existing = _fleets[i];
                if (existing.Id != fleetId)
                {
                    continue;
                }

                _fleets[i] = new FleetPlaceholderState(
                    existing.Id,
                    existing.ClassId,
                    existing.ShipCount,
                    locationSystemId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the fleet already has an active travel leg.
        /// </summary>
        public bool IsFleetTraveling(EntityId fleetId)
        {
            for (var i = 0; i < _activeTravels.Count; i++)
            {
                if (_activeTravels[i].FleetId == fleetId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts a travel leg when the fleet is idle at FromNodeId and arrival is not before departure.
        /// </summary>
        public bool TryStartTravel(ActiveTravel travel)
        {
            if (travel == null
                || travel.FleetId.IsNone
                || travel.FromNodeId.IsEmpty
                || travel.ToNodeId.IsEmpty
                || travel.ArriveTime.Seconds < travel.DepartTime.Seconds
                || IsFleetTraveling(travel.FleetId)
                || !TryGetFleet(travel.FleetId, out var fleet)
                || fleet.LocationSystemId != travel.FromNodeId)
            {
                return false;
            }

            _activeTravels.Add(travel);
            return true;
        }

        /// <summary>
        /// Completes travels whose arrive time is at or before the given sim time.
        /// Completed legs are appended in insertion order. Returns how many completed.
        /// </summary>
        public int CompleteArrivals(GameTime currentTime, List<ActiveTravel> completedBuffer)
        {
            var completed = 0;
            for (var i = 0; i < _activeTravels.Count;)
            {
                var travel = _activeTravels[i];
                if (travel.ArriveTime.Seconds > currentTime.Seconds)
                {
                    i++;
                    continue;
                }

                if (TrySetFleetLocation(travel.FleetId, travel.ToNodeId))
                {
                    completedBuffer?.Add(travel);
                    completed++;
                }

                _activeTravels.RemoveAt(i);
            }

            return completed;
        }
    }
}
