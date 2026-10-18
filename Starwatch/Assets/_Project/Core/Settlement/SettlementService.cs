using System;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Settlement
{
    /// <summary>
    /// Founds and abandons outposts and colonies. An outpost is a cheap claim. A colony pays the rising curve.
    /// </summary>
    internal sealed class SettlementService
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public SettlementService(ObjectDatabaseSO objectDatabase, DomainEventBus events, SimulationClock clock)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Claims a planet in an explored system without counting it as a colony.
        /// </summary>
        public CommandResult FoundOutpost(RunState run, ObserverId observerId, ContentId planetId)
        {
            if (!TryPrepare(run, observerId, planetId, requireScan: false, out var failure))
            {
                return failure;
            }

            if (run.Timeline.TryGetColony(planetId, out _))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "This planet is already claimed.");
            }

            if (!run.Timeline.Resources.TrySpend(SettlementCosts.OutpostCost))
            {
                return CommandResult.Fail(CommandErrorCode.InsufficientResources, "Outpost cost is unpaid.");
            }

            if (!run.Timeline.TryAddColony(new ColonyState(planetId, ColonyPresence.Outpost)))
            {
                run.Timeline.Resources.Grant(SettlementCosts.OutpostCost);
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Outpost could not be founded.");
            }

            _events.Publish(new SettlementFoundedEvent(_clock.CurrentTime, planetId, SettlementKind.Outpost));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Founds a colony, or promotes an outpost, after the planet has been scanned.
        /// </summary>
        public CommandResult FoundColony(RunState run, ObserverId observerId, ContentId planetId)
        {
            if (!TryPrepare(run, observerId, planetId, requireScan: true, out var failure))
            {
                return failure;
            }

            run.Timeline.TryGetColony(planetId, out var existing);
            if (existing != null && existing.Presence == ColonyPresence.Colony)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "This planet is already a colony.");
            }

            var cost = SettlementCosts.ColonyCost(run.Timeline.ColonyCount());
            if (!run.Timeline.Resources.TrySpend(cost))
            {
                return CommandResult.Fail(CommandErrorCode.InsufficientResources, "Settlement cost is unpaid.");
            }

            if (existing != null)
            {
                existing.PromoteToColony();
            }
            else if (!run.Timeline.TryAddColony(new ColonyState(planetId, ColonyPresence.Colony)))
            {
                run.Timeline.Resources.Grant(cost);
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Colony could not be founded.");
            }

            _events.Publish(new SettlementFoundedEvent(_clock.CurrentTime, planetId, SettlementKind.Colony));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Removes an outpost or colony. The founding cost stays spent, and the colony curve counts only claims that remain.
        /// </summary>
        public CommandResult Abandon(RunState run, ContentId planetId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (planetId.IsEmpty || !run.Timeline.TryGetColony(planetId, out var colony))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "This planet is not claimed.");
            }

            var kind = colony.Presence == ColonyPresence.Colony
                ? SettlementKind.Colony
                : SettlementKind.Outpost;
            if (!run.Timeline.TryRemoveColony(planetId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "This planet is not claimed.");
            }

            _events.Publish(new SettlementAbandonedEvent(_clock.CurrentTime, planetId, kind));
            return CommandResult.Ok();
        }

        private bool TryPrepare(
            RunState run,
            ObserverId observerId,
            ContentId planetId,
            bool requireScan,
            out CommandResult failure)
        {
            failure = default;
            if (run == null)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
                return false;
            }

            if (planetId.IsEmpty)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet id is required.");
                return false;
            }

            if (!run.Universe.MapGraph.TryGetNodeForPlanet(planetId, out var hostNode))
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet is not part of this universe.");
                return false;
            }

            if (!_objectDatabase.TryGetPlanet(planetId.Value, out var planet))
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown planet.");
                return false;
            }

            if (planet.SystemId != hostNode.Id)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet host does not match universe map.");
                return false;
            }

            if (!run.Archive.TryGetMapKnowledge(observerId, out var knowledge)
                || !knowledge.HasExploredNode(hostNode.Id))
            {
                failure = CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "Host system must be explored.");
                return false;
            }

            if (requireScan && !knowledge.HasScannedPlanet(planetId))
            {
                failure = CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "Planet must be scanned.");
                return false;
            }

            return true;
        }
    }
}
