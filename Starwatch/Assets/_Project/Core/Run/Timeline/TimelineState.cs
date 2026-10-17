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

        public IReadOnlyList<ColonyState> Colonies => _colonies;

        public ResourceLedger Resources { get; } = new();

        private readonly List<FleetPlaceholderState> _fleets = new();
        private readonly List<ActiveTravel> _activeTravels = new();
        private readonly List<ColonyState> _colonies = new();

        // Derived from _fleets and _activeTravels. TryAddFleet, TrySetFleetLocation, TryStartTravel,
        // and CompleteArrivals keep these current. A new writer, such as removing a fleet, must too.
        // Loading a save should rebuild them after restoring those two lists.
        private readonly Dictionary<ContentId, List<EntityId>> _fleetsByNode = new();
        private readonly HashSet<EntityId> _travelingFleetIds = new();

        public TimelineState(EntityIdFactory entityIds)
        {
            EntityIds = entityIds ?? new EntityIdFactory();
        }

        /// <summary>
        /// Adds a placeholder fleet when class, location, and ship count are valid.
        /// </summary>
        public bool TryAddFleet(ContentId classId, int shipCount, ContentId locationSystemId, out EntityId id)
        {
            id = default;
            if (classId.IsEmpty || locationSystemId.IsEmpty || shipCount <= 0)
            {
                return false;
            }

            id = EntityIds.Next();
            _fleets.Add(new FleetPlaceholderState(id, classId, shipCount, locationSystemId));
            AddFleetToNode(locationSystemId, id);
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

                RemoveFleetFromNode(existing.LocationSystemId, existing.Id);
                _fleets[i] = new FleetPlaceholderState(
                    existing.Id,
                    existing.ClassId,
                    existing.ShipCount,
                    locationSystemId);
                AddFleetToNode(locationSystemId, existing.Id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the fleet already has an active travel leg.
        /// </summary>
        public bool IsFleetTraveling(EntityId fleetId)
        {
            return _travelingFleetIds.Contains(fleetId);
        }

        /// <summary>
        /// Whether any fleet is currently at this node.
        /// </summary>
        public bool HasFleetAt(ContentId nodeId)
        {
            return !nodeId.IsEmpty
                && _fleetsByNode.TryGetValue(nodeId, out var fleets)
                && fleets.Count > 0;
        }

        /// <summary>
        /// Whether a fleet at this node is on an active travel leg.
        /// </summary>
        public bool HasTravelingFleetAt(ContentId nodeId)
        {
            if (nodeId.IsEmpty || !_fleetsByNode.TryGetValue(nodeId, out var fleets))
            {
                return false;
            }

            for (var i = 0; i < fleets.Count; i++)
            {
                if (_travelingFleetIds.Contains(fleets[i]))
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
            _travelingFleetIds.Add(travel.FleetId);
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

                _travelingFleetIds.Remove(travel.FleetId);
                _activeTravels.RemoveAt(i);
            }

            return completed;
        }

        /// <summary>
        /// Colonies only. Outposts do not raise the settlement curve.
        /// </summary>
        public int ColonyCount()
        {
            var count = 0;
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].Presence == ColonyPresence.Colony)
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetColony(ContentId planetId, out ColonyState colony)
        {
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].PlanetId == planetId)
                {
                    colony = _colonies[i];
                    return true;
                }
            }

            colony = null;
            return false;
        }

        public bool TryAddColony(ColonyState colony)
        {
            if (colony == null || colony.PlanetId.IsEmpty || TryGetColony(colony.PlanetId, out _))
            {
                return false;
            }

            _colonies.Add(colony);
            return true;
        }

        /// <summary>
        /// Removes an outpost or colony. Buildings on that claim go with it.
        /// </summary>
        public bool TryRemoveColony(ContentId planetId)
        {
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].PlanetId != planetId)
                {
                    continue;
                }

                _colonies.RemoveAt(i);
                return true;
            }

            return false;
        }

        private void AddFleetToNode(ContentId nodeId, EntityId fleetId)
        {
            if (!_fleetsByNode.TryGetValue(nodeId, out var fleets))
            {
                fleets = new List<EntityId>();
                _fleetsByNode.Add(nodeId, fleets);
            }

            fleets.Add(fleetId);
        }

        private void RemoveFleetFromNode(ContentId nodeId, EntityId fleetId)
        {
            if (!_fleetsByNode.TryGetValue(nodeId, out var fleets))
            {
                return;
            }

            fleets.Remove(fleetId);
            if (fleets.Count == 0)
            {
                _fleetsByNode.Remove(nodeId);
            }
        }
    }
}
