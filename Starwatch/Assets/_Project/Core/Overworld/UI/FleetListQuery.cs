using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Fleet;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Fleet rows for the star map and the open star system.
    /// </summary>
    internal sealed class FleetListQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effects;

        public FleetListQuery(ObjectDatabaseSO database, IEffectQuery effects)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>
        /// Every fleet, in timeline order.
        /// </summary>
        internal List<FleetChoiceModel> All(RunState run, EntityId selectedFleetId)
        {
            List<FleetChoiceModel> rows = new();
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
        internal List<FleetChoiceModel> AtLocation(RunState run, ContentId locationId, EntityId selectedFleetId)
        {
            List<FleetChoiceModel> rows = new();
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

        private FleetChoiceModel Describe(RunState run, FleetState fleet, EntityId selectedFleetId)
        {
            var traveling = run.Timeline.IsFleetTraveling(fleet.Id);
            var speed = FleetSpeed.Effective(fleet, _database, _effects);
            return new FleetChoiceModel(
                fleet.Id,
                fleet.LocationSystemId,
                fleet.ShipCount,
                traveling,
                fleet.Id == selectedFleetId,
                speed,
                Label(fleet, traveling, speed));
        }

        private string Label(FleetState fleet, bool traveling, float speed)
        {
            var composition = StackText(fleet.Stacks[0]);
            for (var i = 1; i < fleet.Stacks.Count; i++)
            {
                composition += ", " + StackText(fleet.Stacks[i]);
            }

            var label = composition + " · " + LocationName(fleet.LocationSystemId);
            if (traveling)
            {
                label += " · traveling";
            }

            return label + " · speed " + speed.ToString("0.##");
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
