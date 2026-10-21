using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Fleet;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Fleet rows for the roster, star map, and the open star system.
    /// </summary>
    internal sealed class FleetListQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effectQuery;
        private readonly SimulationClock _clock;

        public FleetListQuery(
            ObjectDatabaseSO database,
            IEffectQuery effectQuery,
            SimulationClock clock)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _effectQuery = effectQuery ?? throw new ArgumentNullException(nameof(effectQuery));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Every fleet, in timeline order.
        /// </summary>
        internal List<FleetRowModel> All(RunState run, EntityId selectedFleetId)
        {
            List<FleetRowModel> rows = new();
            if (run == null)
            {
                return rows;
            }

            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                rows.Add(Describe(run, fleets[i], selectedFleetId));
            }

            return rows;
        }

        /// <summary>
        /// Fleets whose current location is this node.
        /// </summary>
        internal List<FleetRowModel> AtLocation(RunState run, ContentId locationId, EntityId selectedFleetId)
        {
            List<FleetRowModel> rows = new();
            if (run == null || locationId.IsEmpty)
            {
                return rows;
            }

            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                var fleet = fleets[i];
                if (fleet.LocationSystemId == locationId)
                {
                    rows.Add(Describe(run, fleet, selectedFleetId));
                }
            }

            return rows;
        }

        private FleetRowModel Describe(RunState run, FleetComposition fleet, EntityId selectedFleetId)
        {
            var traveling = TryGetTravel(run, fleet.Id, out var travel);
            var destinationId = traveling ? travel.ToNodeId : default;
            var locationName = LocationName(fleet.LocationSystemId);
            var title = CompositionTitle(fleet);
            var speed = FleetSpeed.Effective(fleet, _database, _effectQuery);
            var route = new FleetRouteModel(
                fleet.LocationSystemId,
                locationName,
                destinationId,
                traveling ? LocationName(destinationId) : string.Empty,
                traveling ? RemainingSeconds(travel.ArriveTime) : 0f,
                traveling);
            return new FleetRowModel(
                fleet.Id,
                route,
                new FleetCompositionModel(fleet.ShipCount, title),
                fleet.Id == selectedFleetId,
                speed,
                Label(title, locationName, traveling, speed));
        }

        private static bool TryGetTravel(RunState run, EntityId fleetId, out ActiveTravel travel)
        {
            var travels = run.Timeline.Fleets.ActiveTravels;
            for (var i = 0; i < travels.Count; i++)
            {
                if (travels[i].FleetId == fleetId)
                {
                    travel = travels[i];
                    return true;
                }
            }

            travel = null;
            return false;
        }

        private float RemainingSeconds(GameTime arriveTime)
        {
            var remaining = arriveTime.Seconds - _clock.CurrentTime.Seconds;
            return remaining > 0d ? (float)remaining : 0f;
        }

        private static string Label(string title, string locationName, bool traveling, float speed)
        {
            var label = title + " · " + locationName;
            if (traveling)
            {
                label += " · traveling";
            }

            return label + " · speed " + speed.ToString("0.##");
        }

        private string CompositionTitle(FleetComposition fleet)
        {
            if (fleet.Stacks.Count == 0)
            {
                return "Fleet";
            }

            var title = StackText(fleet.Stacks[0]);
            for (var i = 1; i < fleet.Stacks.Count; i++)
            {
                title += ", " + StackText(fleet.Stacks[i]);
            }

            return title;
        }

        private string StackText(ShipStack stack)
        {
            return stack.Count + " " + ClassName(stack.ClassId);
        }

        private string ClassName(ContentId classId)
        {
            if (_database.ShipCatalog != null
                && _database.ShipCatalog.TryGet(classId, out var shipClass)
                && shipClass != null
                && !string.IsNullOrEmpty(shipClass.DisplayName))
            {
                return shipClass.DisplayName;
            }

            return classId.Value;
        }

        private string LocationName(ContentId locationId)
        {
            if (_database.TryGetStarSystem(locationId.Value, out var system)
                && !string.IsNullOrEmpty(system.DisplayName))
            {
                return system.DisplayName;
            }

            return locationId.Value;
        }
    }
}
