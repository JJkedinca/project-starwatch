using System;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Fleet
{
    /// <summary>
    /// Explicit fleet composition. Ships join an idle fleet, or a new one when none is waiting.
    /// </summary>
    internal sealed class FleetService
    {
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public FleetService(DomainEventBus events, SimulationClock clock)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Puts a finished ship on an idle fleet at the system, or creates a fleet when none is idle.
        /// TODO: Decide which fleet we want to merge
        /// </summary>
        public bool AddBuiltShip(RunState run, ContentId systemId, ContentId classId, out EntityId fleetId)
        {
            fleetId = default;
            if (run == null || systemId.IsEmpty || classId.IsEmpty)
            {
                return false;
            }

            var atSystem = run.Timeline.Fleets.FleetsAt(systemId);
            for (var i = 0; i < atSystem.Count; i++)
            {
                var id = atSystem[i];
                if (run.Timeline.Fleets.IsFleetTraveling(id))
                {
                    continue;
                }

                if (!run.Timeline.Fleets.TryAddShips(id, classId, 1))
                {
                    return false;
                }

                fleetId = id;
                return true;
            }

            if (!run.Timeline.Fleets.TryAddFleet(classId, 1, systemId, out fleetId))
            {
                return false;
            }

            _events.Publish(new FleetCreatedEvent(Now, fleetId, systemId));
            return true;
        }

        /// <summary>
        /// Folds the source fleet into the target. Both must be idle at the same node. The target id is kept.
        /// </summary>
        public CommandResult Merge(RunState run, EntityId targetFleetId, EntityId sourceFleetId)
        {
            if (!TryIdlePair(run, targetFleetId, sourceFleetId, out var target, out var source, out var ready))
            {
                return ready;
            }

            if (target.LocationSystemId != source.LocationSystemId)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Fleets are not at the same node.");
            }

            if (!run.Timeline.Fleets.TryMergeFleets(target.Id, source.Id))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Fleet could not be merged.");
            }

            _events.Publish(new FleetMergedEvent(Now, target.Id, source.Id));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Moves some ships of one class into a new fleet at the same node.
        /// </summary>
        public CommandResult Split(RunState run, EntityId fleetId, ContentId classId, int count, out EntityId newFleetId)
        {
            newFleetId = default;
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (classId.IsEmpty || count <= 0)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Choose a class and a ship count.");
            }

            if (!run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown fleet.");
            }

            if (run.Timeline.Fleets.IsFleetTraveling(fleetId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Fleet is traveling.");
            }

            if (fleet.CountOf(classId) < count)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "The fleet does not have that many ships.");
            }

            if (fleet.ShipCount - count <= 0)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "That would split the whole fleet.");
            }

            if (!run.Timeline.Fleets.TrySplitFleet(fleetId, classId, count, out newFleetId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Fleet could not be split.");
            }

            _events.Publish(new FleetSplitEvent(Now, fleetId, newFleetId));
            return CommandResult.Ok();
        }

        private static bool TryIdlePair(
            RunState run,
            EntityId targetFleetId,
            EntityId sourceFleetId,
            out FleetComposition target,
            out FleetComposition source,
            out CommandResult failure)
        {
            target = null;
            source = null;
            if (run == null)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
                return false;
            }

            if (targetFleetId == sourceFleetId)
            {
                failure = CommandResult.Fail(CommandErrorCode.NotAllowed, "Choose two fleets.");
                return false;
            }

            if (!run.Timeline.Fleets.TryGetFleet(targetFleetId, out target) || !run.Timeline.Fleets.TryGetFleet(sourceFleetId, out source))
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown fleet.");
                return false;
            }

            if (run.Timeline.Fleets.IsFleetTraveling(targetFleetId) || run.Timeline.Fleets.IsFleetTraveling(sourceFleetId))
            {
                failure = CommandResult.Fail(CommandErrorCode.NotAllowed, "A traveling fleet cannot merge.");
                return false;
            }

            failure = default;
            return true;
        }

        private GameTime Now => _clock.CurrentTime;
    }
}
