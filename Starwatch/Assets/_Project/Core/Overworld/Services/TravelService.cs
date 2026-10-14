using EndlessHeros.Core.Commands;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Overworld.Services
{
    /// <summary>
    /// Starts fleet travel when route and exploration rules allow.
    /// </summary>
    public sealed class TravelService
    {
        private readonly RouteService _routeService;
        private readonly SimulationClock _clock;

        public TravelService(RouteService routeService, SimulationClock clock)
        {
            _routeService = routeService;
            _clock = clock;
        }

        /// <summary>
        /// Begins travel for a fleet to an adjacent explored destination.
        /// </summary>
        public CommandResult StartTravel(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (destinationNodeId.IsEmpty)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Destination is required.");
            }

            if (!run.Timeline.TryGetFleet(fleetId, out var fleet))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown fleet.");
            }

            if (run.Timeline.IsFleetTraveling(fleetId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Fleet is already traveling.");
            }

            if (!_routeService.AreAdjacent(run.Universe.MapGraph, fleet.LocationSystemId, destinationNodeId))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidRoute, "Destination is not adjacent.");
            }

            if (!run.Archive.TryGetMapKnowledge(observerId, out var knowledge)
                || !knowledge.HasExploredNode(destinationNodeId))
            {
                return CommandResult.Fail(
                    CommandErrorCode.PrerequisiteNotMet,
                    "Destination must be explored before travel.");
            }

            if (!_routeService.TryGetTravelDuration(
                    run.Universe.MapGraph,
                    fleet.LocationSystemId,
                    destinationNodeId,
                    out var duration))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidRoute, "Route duration missing.");
            }

            var depart = _clock != null ? _clock.CurrentTime : GameTime.Zero;
            var arrive = depart.Add(duration);
            var travel = new ActiveTravel(
                fleetId,
                fleet.LocationSystemId,
                destinationNodeId,
                depart,
                arrive);

            if (!run.Timeline.TryStartTravel(travel))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Travel could not start.");
            }

            return CommandResult.Ok();
        }
    }
}
