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
    /// Places and upgrades buildings on a colony, within the planet footprint.
    /// </summary>
    internal sealed class BuildingService
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly SettlementCatalogSO _catalog;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public BuildingService(ObjectDatabaseSO objectDatabase, DomainEventBus events, SimulationClock clock)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _catalog = objectDatabase.SettlementCatalog
                ?? throw new ArgumentException(
                    "ObjectDatabaseSO.SettlementCatalog is required.",
                    nameof(objectDatabase));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Builds level 1 of a building. Off-specialization buildings are allowed.
        /// </summary>
        public CommandResult Place(RunState run, ContentId planetId, ContentId buildingId)
        {
            if (!TryGetColonyAndDefinition(run, planetId, buildingId, out var colony, out var definition, out var failure))
            {
                return failure;
            }

            if (colony.FindBuilding(buildingId) != null)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Building is already placed.");
            }

            if (UsedFootprint(colony) + definition.Footprint > FootprintBudget(planetId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Planet footprint is full.");
            }

            if (!run.Timeline.Resources.TrySpend(definition.BuildCost.Amounts))
            {
                return CommandResult.Fail(CommandErrorCode.InsufficientResources, "Build cost is unpaid.");
            }

            colony.AddBuilding(new ColonyBuildingState(buildingId, 1));
            _events.Publish(new BuildingChangedEvent(_clock.CurrentTime, planetId, buildingId, 1));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Raises a placed building by one level, up to its authored maximum.
        /// </summary>
        public CommandResult Upgrade(RunState run, ContentId planetId, ContentId buildingId)
        {
            if (!TryGetColonyAndDefinition(run, planetId, buildingId, out var colony, out var definition, out var failure))
            {
                return failure;
            }

            var building = colony.FindBuilding(buildingId);
            if (building == null)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Building is not placed.");
            }

            if (building.Level >= definition.MaxLevel)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Building is at max level.");
            }

            if (!definition.TryGetUpgradeCost(building.Level, out var upgradeCost))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Upgrade cost is missing.");
            }

            if (!run.Timeline.Resources.TrySpend(upgradeCost.Amounts))
            {
                return CommandResult.Fail(CommandErrorCode.InsufficientResources, "Upgrade cost is unpaid.");
            }

            building.IncreaseLevel();
            _events.Publish(new BuildingChangedEvent(_clock.CurrentTime, planetId, buildingId, building.Level));
            return CommandResult.Ok();
        }

        private bool TryGetColonyAndDefinition(
            RunState run,
            ContentId planetId,
            ContentId buildingId,
            out ColonyState colony,
            out BuildingDefinitionSO definition,
            out CommandResult failure)
        {
            colony = null;
            definition = null;
            failure = default;
            if (run == null)
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
                return false;
            }

            if (!_catalog.TryGetBuilding(buildingId, out definition))
            {
                failure = CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown building.");
                return false;
            }

            if (!run.Timeline.TryGetColony(planetId, out colony) || colony.Presence != ColonyPresence.Colony)
            {
                failure = CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "A colony is required.");
                return false;
            }

            return true;
        }

        private int FootprintBudget(ContentId planetId)
        {
            if (_objectDatabase.TryGetPlanet(planetId.Value, out var planet))
            {
                return planet.FootprintBudget;
            }

            return 0;
        }

        private int UsedFootprint(ColonyState colony)
        {
            var used = 0;
            foreach (var placed in colony.Buildings.Values)
            {
                if (_catalog.TryGetBuilding(placed.BuildingId, out var definition))
                {
                    used += definition.Footprint;
                }
            }

            return used;
        }
    }
}
