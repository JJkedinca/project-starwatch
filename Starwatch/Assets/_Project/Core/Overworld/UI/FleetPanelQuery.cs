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
    /// Read model for the fleet panel. Travel speed is the slowest class times the research multiplier.
    /// </summary>
    internal sealed class FleetPanelQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effects;

        public FleetPanelQuery(ObjectDatabaseSO database, IEffectQuery effects)
        {
            _database = database;
            _effects = effects;
        }

        internal FleetPanelModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null
                || selection == null
                || !selection.InStarSystem
                || selection.SelectedFleetId.IsNone)
            {
                return FleetPanelModel.Closed;
            }

            if (!run.Timeline.TryGetFleet(selection.SelectedFleetId, out var fleet))
            {
                return FleetPanelModel.Closed;
            }

            List<FleetStackRow> stacks = new();
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var stack = fleet.Stacks[i];
                DescribeClass(stack.ClassId, out var displayName, out var hasClass);
                stacks.Add(new FleetStackRow(
                    stack.ClassId,
                    stack.Count,
                    FleetSpeed.ClassFactor(_database, stack.ClassId),
                    displayName,
                    hasClass));
            }

            var traveling = run.Timeline.IsFleetTraveling(fleet.Id);
            return new FleetPanelModel(
                true,
                fleet.Id,
                fleet.LocationSystemId,
                traveling,
                FleetSpeed.Effective(fleet, _database, _effects),
                !traveling && fleet.ShipCount > 1,
                stacks,
                MergeCandidates(run, fleet, traveling));
        }

        private static List<FleetMergeCandidate> MergeCandidates(RunState run, FleetState fleet, bool traveling)
        {
            List<FleetMergeCandidate> candidates = new();
            if (traveling)
            {
                return candidates;
            }

            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                var other = fleets[i];
                if (other.Id == fleet.Id
                    || other.LocationSystemId != fleet.LocationSystemId
                    || run.Timeline.IsFleetTraveling(other.Id))
                {
                    continue;
                }

                candidates.Add(new FleetMergeCandidate(other.Id, other.ShipCount));
            }

            return candidates;
        }

        private void DescribeClass(ContentId classId, out string displayName, out bool hasClass)
        {
            displayName = classId.Value;
            hasClass = false;
            if (_database == null
                || _database.ShipCatalog == null
                || !_database.ShipCatalog.TryGet(classId, out var shipClass)
                || shipClass == null)
            {
                return;
            }

            hasClass = true;
            if (!string.IsNullOrEmpty(shipClass.DisplayName))
            {
                displayName = shipClass.DisplayName;
            }
        }
    }
}
