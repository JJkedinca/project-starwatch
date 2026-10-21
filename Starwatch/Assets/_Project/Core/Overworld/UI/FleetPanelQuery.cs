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
    /// Read model for the fleet bar and details panel.
    /// The bar shows whenever a fleet is selected. Details open only with <see cref="RightColumnFocus.FleetDetails"/>.
    /// </summary>
    internal sealed class FleetPanelQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effectQuery;
        private readonly SimulationClock _clock;

        public FleetPanelQuery(
            ObjectDatabaseSO database,
            IEffectQuery effectQuery,
            SimulationClock clock)
        {
            _database = database;
            _effectQuery = effectQuery;
            _clock = clock;
        }

        internal FleetPanelModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null || selection == null || selection.SelectedFleetId.IsNone)
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
                stacks.Add(new FleetStackRow(
                    stack.ClassId,
                    stack.Count,
                    OverworldNames.ShipClass(_database, stack.ClassId)));
            }

            var traveling = TryGetTravel(run, fleet.Id, out var travel);
            var destinationId = traveling ? travel.ToNodeId : default;
            var route = new FleetRouteModel(
                fleet.LocationSystemId,
                OverworldNames.StarSystem(_database, fleet.LocationSystemId),
                destinationId,
                traveling ? OverworldNames.StarSystem(_database, destinationId) : string.Empty,
                traveling ? OverworldNames.RemainingSeconds(_clock, travel.ArriveTime) : 0f,
                traveling);
            return new FleetPanelModel(
                hasFleet: true,
                isOpen: selection.RightColumnFocus == RightColumnFocus.FleetDetails,
                fleet.Id,
                route,
                FleetSpeed.Effective(fleet, _database, _effectQuery),
                !traveling && fleet.ShipCount > 1,
                stacks,
                MergeCandidates(run, fleet, traveling));
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
    }
}
