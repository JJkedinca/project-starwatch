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
        /// Begins travel for a fleet along a known adjacent route. Arrival explores the destination.
        /// </summary>
        public CommandResult StartTravel(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId)
        {
            var evaluation = EvaluateTravel(
                run,
                observerId,
                fleetId,
                destinationNodeId,
                out var fleet,
                out var duration);
            if (!evaluation.Success)
            {
                return evaluation;
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

        /// <summary>
        /// Returns whether StartTravel would accept this leg.
        /// </summary>
        public bool CanStartTravel(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId)
        {
            return EvaluateTravel(
                run,
                observerId,
                fleetId,
                destinationNodeId,
                out _,
                out _).Success;
        }

        private CommandResult EvaluateTravel(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId,
            out FleetPlaceholderState fleet,
            out float duration)
        {
            fleet = null;
            duration = 0f;
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (destinationNodeId.IsEmpty)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Destination is required.");
            }

            if (!run.Timeline.TryGetFleet(fleetId, out fleet))
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
                || !knowledge.HasKnownRoute(
                    RouteService.MakeRouteKey(fleet.LocationSystemId, destinationNodeId)))
            {
                return CommandResult.Fail(
                    CommandErrorCode.PrerequisiteNotMet,
                    "Route is not known.");
            }

            if (!_routeService.TryGetTravelDuration(
                    run.Universe.MapGraph,
                    fleet.LocationSystemId,
                    destinationNodeId,
                    out duration))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidRoute, "Route duration missing.");
            }

            return CommandResult.Ok();
        }
    }
}
