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
    /// Stays closed on the star map. Opens only while a star system is open and a fleet is selected.
    /// </summary>
    internal sealed class FleetPanelQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effectQuery;

        public FleetPanelQuery(ObjectDatabaseSO database, IEffectQuery effectQuery)
        {
            _database = database;
            _effectQuery = effectQuery;
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

            if (!run.Timeline.Fleets.TryGetFleet(selection.SelectedFleetId, out var fleet))
            {
                return FleetPanelModel.Closed;
            }

            List<FleetStackRow> stacks = new();
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var stack = fleet.Stacks[i];
                stacks.Add(new FleetStackRow(stack.ClassId, stack.Count, ClassName(stack.ClassId)));
            }

            var traveling = run.Timeline.Fleets.IsFleetTraveling(fleet.Id);
            return new FleetPanelModel(
                true,
                fleet.Id,
                fleet.LocationSystemId,
                traveling,
                FleetSpeed.Effective(fleet, _database, _effectQuery),
                !traveling && fleet.ShipCount > 1,
                stacks,
                MergeCandidates(run, fleet, traveling));
        }

        private static List<FleetMergeCandidate> MergeCandidates(RunState run, FleetComposition fleet, bool traveling)
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
                    || run.Timeline.Fleets.IsFleetTraveling(other.Id))
                {
                    continue;
                }

                candidates.Add(new FleetMergeCandidate(other.Id, other.ShipCount));
            }

            return candidates;
        }

        private string ClassName(ContentId classId)
        {
            if (_database == null
                || _database.ShipCatalog == null
                || !_database.ShipCatalog.TryGet(classId, out var shipClass)
                || shipClass == null
                || string.IsNullOrEmpty(shipClass.DisplayName))
            {
                return classId.Value;
            }

            return shipClass.DisplayName;
        }
    }
}
