using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// Mutable empire state for the current timeline attempt. Resets on rewind.
    /// Owns its collections; must not alias universe or archive collections.
    /// Home system lives on Universe.StartingEmpire — not duplicated here.
    /// </summary>
    public sealed class TimelineState
    {
        public EntityIdFactory EntityIds { get; }

        public IReadOnlyList<FleetState> Fleets => _fleets;

        public IReadOnlyList<ActiveTravel> ActiveTravels => _activeTravels;

        public IReadOnlyList<ColonyState> Colonies => _colonies;

        public ResourceLedger Resources { get; } = new();

        public ResearchProgress Research { get; } = new();

        private readonly List<FleetState> _fleets = new();
        private readonly List<ActiveTravel> _activeTravels = new();
        private readonly List<ColonyState> _colonies = new();

        // Derived from _fleets and _activeTravels. TryAddFleet, TrySetFleetLocation, TryRemoveFleet,
        // TryStartTravel, and CompleteArrivals keep these current.
        // Loading a save should rebuild them after restoring those two lists.
        private readonly Dictionary<ContentId, List<EntityId>> _fleetsByNode = new();
        private readonly HashSet<EntityId> _travelingFleetIds = new();

        public TimelineState(EntityIdFactory entityIds)
        {
            EntityIds = entityIds ?? new EntityIdFactory();
        }

        /// <summary>
        /// Adds a fleet of one class when the class, location, and ship count are valid.
        /// </summary>
        public bool TryAddFleet(ContentId classId, int shipCount, ContentId locationSystemId, out EntityId id)
        {
            if (classId.IsEmpty || shipCount <= 0)
            {
                id = default;
                return false;
            }

            return TryAddFleet(new[] { new ShipStack(classId, shipCount) }, locationSystemId, out id);
        }

        /// <summary>
        /// Adds a fleet with the given stacks when the location and stacks are valid.
        /// </summary>
        public bool TryAddFleet(IReadOnlyList<ShipStack> stacks, ContentId locationSystemId, out EntityId id)
        {
            id = default;
            if (locationSystemId.IsEmpty || !HasShips(stacks))
            {
                return false;
            }

            id = EntityIds.Next();
            _fleets.Add(new FleetState(id, locationSystemId, stacks));
            AddFleetToNode(locationSystemId, id);
            return true;
        }

        /// <summary>
        /// Adds ships to an idle fleet, stacking with the same class when one is already there.
        /// </summary>
        public bool TryAddShips(EntityId fleetId, ContentId classId, int count)
        {
            if (classId.IsEmpty || count <= 0 || IsFleetTraveling(fleetId) || !TryGetFleet(fleetId, out var fleet))
            {
                return false;
            }

            var stacks = new List<ShipStack>(fleet.Stacks.Count + 1);
            var stacked = false;
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var stack = fleet.Stacks[i];
                if (stack.ClassId == classId)
                {
                    stacks.Add(new ShipStack(classId, stack.Count + count));
                    stacked = true;
                }
                else
                {
                    stacks.Add(stack);
                }
            }

            if (!stacked)
            {
                stacks.Add(new ShipStack(classId, count));
            }

            return ReplaceFleet(fleet, fleet.LocationSystemId, stacks);
        }

        /// <summary>
        /// Removes ships of one class from an idle fleet. Fails when the fleet is traveling or the removal would leave it empty.
        /// </summary>
        public bool TryRemoveShips(EntityId fleetId, ContentId classId, int count)
        {
            if (classId.IsEmpty || count <= 0 || IsFleetTraveling(fleetId) || !TryGetFleet(fleetId, out var fleet))
            {
                return false;
            }

            var found = fleet.CountOf(classId);
            if (found < count || fleet.ShipCount - count <= 0)
            {
                return false;
            }

            var stacks = new List<ShipStack>(fleet.Stacks.Count);
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var stack = fleet.Stacks[i];
                if (stack.ClassId != classId)
                {
                    stacks.Add(stack);
                    continue;
                }

                var remaining = stack.Count - count;
                if (remaining > 0)
                {
                    stacks.Add(new ShipStack(classId, remaining));
                }
            }

            return ReplaceFleet(fleet, fleet.LocationSystemId, stacks);
        }

        /// <summary>
        /// Moves every ship from the source onto the target, then removes the source. Both fleets must be idle at the same node.
        /// </summary>
        public bool TryMergeFleets(EntityId targetFleetId, EntityId sourceFleetId)
        {
            if (targetFleetId == sourceFleetId
                || IsFleetTraveling(targetFleetId)
                || IsFleetTraveling(sourceFleetId)
                || !TryGetFleet(targetFleetId, out var target)
                || !TryGetFleet(sourceFleetId, out var source)
                || target.LocationSystemId != source.LocationSystemId)
            {
                return false;
            }

            var original = target.Stacks;
            if (!ReplaceFleet(target, target.LocationSystemId, Combine(original, source.Stacks)))
            {
                return false;
            }

            if (TryRemoveFleet(sourceFleetId))
            {
                return true;
            }

            ReplaceFleet(target, target.LocationSystemId, original);
            return false;
        }

        /// <summary>
        /// Moves ships of one class from an idle fleet into a new fleet at the same node.
        /// </summary>
        public bool TrySplitFleet(EntityId fleetId, ContentId classId, int count, out EntityId newFleetId)
        {
            newFleetId = default;
            if (classId.IsEmpty || count <= 0 || IsFleetTraveling(fleetId) || !TryGetFleet(fleetId, out var fleet))
            {
                return false;
            }

            if (fleet.CountOf(classId) < count || fleet.ShipCount - count <= 0)
            {
                return false;
            }

            var createdId = EntityIds.Next();
            var created = new FleetState(
                createdId,
                fleet.LocationSystemId,
                new[] { new ShipStack(classId, count) });
            if (!ReplaceFleet(fleet, fleet.LocationSystemId, Reduced(fleet, classId, count)))
            {
                return false;
            }

            _fleets.Add(created);
            AddFleetToNode(created.LocationSystemId, createdId);
            newFleetId = createdId;
            return true;
        }

        /// <summary>
        /// Removes an idle fleet and its index entry.
        /// </summary>
        public bool TryRemoveFleet(EntityId fleetId)
        {
            if (IsFleetTraveling(fleetId))
            {
                return false;
            }

            for (var i = 0; i < _fleets.Count; i++)
            {
                if (_fleets[i].Id != fleetId)
                {
                    continue;
                }

                RemoveFleetFromNode(_fleets[i].LocationSystemId, fleetId);
                _fleets.RemoveAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Looks up a fleet by id.
        /// </summary>
        public bool TryGetFleet(EntityId fleetId, out FleetState fleet)
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

                return ReplaceFleet(existing, locationSystemId, existing.Stacks);
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

        private static List<ShipStack> Combine(IReadOnlyList<ShipStack> target, IReadOnlyList<ShipStack> source)
        {
            var stacks = new List<ShipStack>(target.Count + source.Count);
            for (var i = 0; i < target.Count; i++)
            {
                stacks.Add(target[i]);
            }

            for (var i = 0; i < source.Count; i++)
            {
                var added = false;
                for (var slot = 0; slot < stacks.Count; slot++)
                {
                    if (stacks[slot].ClassId != source[i].ClassId)
                    {
                        continue;
                    }

                    stacks[slot] = new ShipStack(source[i].ClassId, stacks[slot].Count + source[i].Count);
                    added = true;
                    break;
                }

                if (!added)
                {
                    stacks.Add(source[i]);
                }
            }

            return stacks;
        }

        private static List<ShipStack> Reduced(FleetState fleet, ContentId classId, int count)
        {
            var stacks = new List<ShipStack>(fleet.Stacks.Count);
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var stack = fleet.Stacks[i];
                if (stack.ClassId != classId)
                {
                    stacks.Add(stack);
                    continue;
                }

                var remaining = stack.Count - count;
                if (remaining > 0)
                {
                    stacks.Add(new ShipStack(classId, remaining));
                }
            }

            return stacks;
        }

        private static bool HasShips(IReadOnlyList<ShipStack> stacks)
        {
            if (stacks == null)
            {
                return false;
            }

            for (var i = 0; i < stacks.Count; i++)
            {
                if (!stacks[i].ClassId.IsEmpty && stacks[i].Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ReplaceFleet(FleetState existing, ContentId locationSystemId, IReadOnlyList<ShipStack> stacks)
        {
            for (var i = 0; i < _fleets.Count; i++)
            {
                if (_fleets[i].Id != existing.Id)
                {
                    continue;
                }

                if (existing.LocationSystemId != locationSystemId)
                {
                    RemoveFleetFromNode(existing.LocationSystemId, existing.Id);
                    AddFleetToNode(locationSystemId, existing.Id);
                }

                _fleets[i] = new FleetState(existing.Id, locationSystemId, stacks);
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
