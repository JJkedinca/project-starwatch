using System;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Fleet
{
    /// <summary>
    /// Queues and cancels ship construction at a colony that has a building marked for ship construction.
    /// </summary>
    internal sealed class ShipProductionService
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effectQuery;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public ShipProductionService(
            ObjectDatabaseSO database,
            IEffectQuery effectQuery,
            DomainEventBus events,
            SimulationClock clock)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _effectQuery = effectQuery ?? throw new ArgumentNullException(nameof(effectQuery));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Pays the class cost and appends one ship to the colony queue.
        /// </summary>
        public CommandResult Queue(RunState run, ContentId planetId, ContentId classId)
        {
            if (!TryReadyColony(run, planetId, out var colony, out var failure))
            {
                return failure;
            }

            if (_database.ShipCatalog == null || !_database.ShipCatalog.TryGet(classId, out var shipClass) || shipClass == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown ship class.");
            }

            if (shipClass.BuildSeconds <= 0f)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "That class has no build time.");
            }

            if (!string.IsNullOrWhiteSpace(shipClass.RequiredCapabilityId)
                && !_effectQuery.HasCapability(shipClass.RequiredCapabilityId))
            {
                return CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "That class is still locked.");
            }

            if (!colony.CanEnqueueBuild)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "The build queue is full.");
            }

            var cost = shipClass.BuildCost.Amounts;
            if (!run.Timeline.Resources.TrySpend(cost))
            {
                return CommandResult.Fail(CommandErrorCode.InsufficientResources, "Build cost is unpaid.");
            }

            colony.EnqueueBuild(new ShipBuildOrder(classId, shipClass.BuildSeconds));
            var time = _clock.CurrentTime;
            _events.Publish(new ShipQueuedEvent(time, planetId, classId));
            _events.Publish(new ResourceChangedEvent(time));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Removes a queued ship and refunds its cost. Abandoning a colony does not refund.
        /// </summary>
        public CommandResult Cancel(RunState run, ContentId planetId, int queueIndex)
        {
            if (!TryColony(run, planetId, out var colony, out var failure))
            {
                return failure;
            }

            if (!colony.TryTakeBuild(queueIndex, out var order))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "That build order is not in the queue.");
            }

            if (_database.ShipCatalog != null && _database.ShipCatalog.TryGet(order.ClassId, out var shipClass) && shipClass != null)
            {
                run.Timeline.Resources.Grant(shipClass.BuildCost.Amounts);
            }

            var time = _clock.CurrentTime;
            _events.Publish(new ResourceChangedEvent(time));
            return CommandResult.Ok();
        }

        private bool TryReadyColony(RunState run, ContentId planetId, out ColonyState colony, out CommandResult failure)
        {
            if (!TryColony(run, planetId, out colony, out failure))
            {
                return false;
            }

            if (colony.Presence != ColonyPresence.Colony)
            {
                failure = CommandResult.Fail(CommandErrorCode.NotAllowed, "An outpost cannot build ships.");
                return false;
            }

            if (!EnablesShipConstruction(colony))
            {
                failure = CommandResult.Fail(CommandErrorCode.NotAllowed, "The colony has no shipyard.");
                return false;
            }

            return true;
        }

        private static bool TryColony(RunState run, ContentId planetId, out ColonyState colony, out CommandResult failure)
        {
            colony = null;
            if (run == null)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
                return false;
            }

            if (planetId.IsEmpty || !run.Timeline.TryGetColony(planetId, out colony))
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown colony.");
                return false;
            }

            failure = default;
            return true;
        }

        internal bool EnablesShipConstruction(ColonyState colony)
        {
            var catalog = _database.SettlementCatalog;
            if (catalog == null)
            {
                return false;
            }

            foreach (var placed in colony.Buildings.Values)
            {
                if (catalog.TryGetBuilding(placed.BuildingId, out var definition)
                    && definition != null
                    && definition.EnablesShipConstruction)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
