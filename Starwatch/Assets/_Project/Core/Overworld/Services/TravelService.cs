using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Fleet;

namespace ProjectStarwatch.Overworld.Services
{
    /// <summary>
    /// Starts fleet travel when route and exploration rules allow.
    /// </summary>
    internal sealed class TravelService
    {
        private readonly RouteService _routeService;
        private readonly SimulationClock _clock;
        private readonly DomainEventBus _events;
        private readonly IEffectQuery _effectQuery;
        private readonly ObjectDatabaseSO _database;

        public TravelService(
            RouteService routeService,
            SimulationClock clock,
            DomainEventBus events,
            IEffectQuery effectQuery,
            ObjectDatabaseSO database)
        {
            _routeService = routeService;
            _clock = clock;
            _events = events ?? throw new System.ArgumentNullException(nameof(events));
            _effectQuery = effectQuery;
            _database = database;
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
            var travel = new ActiveTravel(fleetId, fleet.LocationSystemId, destinationNodeId, depart, arrive);

            if (!run.Timeline.Fleets.TryStartTravel(travel))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Travel could not start.");
            }

            _events.Publish(new FleetTravelEvent(
                depart,
                fleetId,
                fleet.LocationSystemId,
                destinationNodeId,
                FleetTravelKind.Started));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Returns whether a known route links these adjacent nodes.
        /// Ignores whether a fleet is already traveling.
        /// </summary>
        public bool IsKnownAdjacentDestination(
            RunState run,
            ObserverId observerId,
            ContentId fromNodeId,
            ContentId destinationNodeId)
        {
            if (run == null || fromNodeId.IsEmpty || destinationNodeId.IsEmpty)
            {
                return false;
            }

            if (!_routeService.AreAdjacent(run.Universe.MapGraph, fromNodeId, destinationNodeId))
            {
                return false;
            }

            return run.Archive.TryGetMapKnowledge(observerId, out var knowledge)
                && knowledge.HasKnownRoute(RouteService.MakeRouteKey(fromNodeId, destinationNodeId));
        }

        /// <summary>
        /// Returns whether StartTravel would accept this leg.
        /// </summary>
        public bool CanStartTravel(RunState run, ObserverId observerId, EntityId fleetId, ContentId destinationNodeId)
        {
            return EvaluateTravel(run, observerId, fleetId, destinationNodeId, out _, out _).Success;
        }

        /// <summary>
        /// Seconds until the selected fleet would arrive, when the leg is allowed.
        /// </summary>
        public bool TryGetTripSeconds(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId,
            out float seconds)
        {
            return EvaluateTravel(run, observerId, fleetId, destinationNodeId, out _, out seconds).Success;
        }

        private CommandResult EvaluateTravel(
            RunState run,
            ObserverId observerId,
            EntityId fleetId,
            ContentId destinationNodeId,
            out FleetComposition fleet,
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

            if (!run.Timeline.Fleets.TryGetFleet(fleetId, out fleet))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown fleet.");
            }

            if (run.Timeline.Fleets.IsFleetTraveling(fleetId))
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
                return CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "Route is not known.");
            }

            if (!_routeService.TryGetTravelDuration(
                    run.Universe.MapGraph,
                    fleet.LocationSystemId,
                    destinationNodeId,
                    out var routeSeconds))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidRoute, "Route duration missing.");
            }

            duration = routeSeconds / FleetSpeed.Effective(fleet, _database, _effectQuery);
            return CommandResult.Ok();
        }
    }
}
