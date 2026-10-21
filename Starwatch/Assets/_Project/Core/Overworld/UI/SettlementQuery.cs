using System;
using System.Collections.Generic;
using System.Text;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Fleet;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Read model for the settlement panel on an open star system.
    /// Base stats and current production come from <see cref="ProductionService"/>.
    /// </summary>
    internal sealed class SettlementQuery
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly ProductionService _productionService;
        private readonly ShipProductionService _shipProductionService;

        public SettlementQuery(
            ObjectDatabaseSO objectDatabase,
            ProductionService productionService,
            ShipProductionService shipProductionService)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _shipProductionService = shipProductionService
                ?? throw new ArgumentNullException(nameof(shipProductionService));
        }

        /// <summary>
        /// Stockpile for the empire, and the selected planet's resources, development, commitment, and buildings.
        /// Development stays hidden until the planet is scanned. Founding a colony waits for that scan.
        /// The starting colony is already founded, so it can place buildings and use a shipyard before the scan.
        /// </summary>
        internal SettlementPanelModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null
                || selection == null
                || !selection.InStarSystem
                || selection.RightColumnFocus != RightColumnFocus.Target)
            {
                return SettlementPanelModel.Hidden;
            }

            var planetId = selection.SelectedPlanetId;
            var resources = Stockpile(run.Timeline.Resources);
            PlanetDefinition planet = null;
            var showPlanet = !planetId.IsEmpty && _objectDatabase.TryGetPlanet(planetId.Value, out planet);
            if (!showPlanet)
            {
                return new SettlementPanelModel(
                    showPanel: true,
                    resources,
                    default,
                    showPlanet: false,
                    default,
                    string.Empty,
                    string.Empty,
                    default,
                    Array.Empty<BuildingRowModel>(),
                    showShipyardTab: false);
            }

            run.Timeline.TryGetColony(planetId, out var colony);
            var scanned = IsScanned(run, planetId);
            var used = UsedFootprint(colony);
            var presence = PresenceKind(colony);
            var chips = scanned ? DepositChips(planet) : Array.Empty<string>();
            var header = new SettlementHeaderModel(
                planetId,
                planet.DisplayName,
                scanned,
                canScan: !scanned,
                presence,
                ColonyLevel(colony),
                used,
                planet.FootprintBudget,
                chips,
                canAbandon: colony != null);
            return new SettlementPanelModel(
                showPanel: true,
                resources,
                scanned ? DescribeEconomy(planet, colony) : default,
                showPlanet: true,
                header,
                scanned ? DevelopmentText(planet, colony) : "Scan to reveal development.",
                StatusText(colony),
                Commitment(run, colony, scanned),
                BuildingRows(run, planet, colony),
                showShipyardTab: CanShowShipyardTab(colony));
        }

        private PlanetEconomy DescribeEconomy(PlanetDefinition planet, ColonyState colony)
        {
            var quote = _productionService.Quote(colony, planet);
            return new PlanetEconomy(_productionService.ModifiedBase(planet), quote.Output, quote.Upkeep);
        }

        private string DevelopmentText(PlanetDefinition planet, ColonyState colony)
        {
            var builder = new StringBuilder();
            builder.Append("Footprint ");
            builder.Append(UsedFootprint(colony));
            builder.Append('/');
            builder.Append(planet.FootprintBudget);
            builder.Append("\nIndustry x");
            builder.Append(FormatQuantity(planet.Modifiers.Multiplier(ResourceKind.Industry)));
            builder.Append(", Energy x");
            builder.Append(FormatQuantity(planet.Modifiers.Multiplier(ResourceKind.Energy)));
            builder.Append(", Science x");
            builder.Append(FormatQuantity(planet.Modifiers.Multiplier(ResourceKind.Science)));
            builder.Append(", Strategic x");
            builder.Append(FormatQuantity(planet.Modifiers.Multiplier(ResourceKind.Strategic)));
            builder.Append("\nDeposits: ");
            builder.Append(DepositText(planet));
            return builder.ToString();
        }

        private static string StatusText(ColonyState colony)
        {
            if (colony == null)
            {
                return "Unclaimed";
            }

            return colony.Presence == ColonyPresence.Colony ? "Colony" : "Outpost";
        }

        private static SettlementPresenceKind PresenceKind(ColonyState colony)
        {
            if (colony == null)
            {
                return SettlementPresenceKind.None;
            }

            return colony.Presence == ColonyPresence.Colony
                ? SettlementPresenceKind.Colony
                : SettlementPresenceKind.Outpost;
        }

        /// <summary>
        /// Highest placed building level, or 1 for a founded colony with no buildings yet.
        /// </summary>
        private static int ColonyLevel(ColonyState colony)
        {
            if (colony == null || colony.Presence != ColonyPresence.Colony)
            {
                return 0;
            }

            var level = 1;
            foreach (var building in colony.Buildings.Values)
            {
                if (building.Level > level)
                {
                    level = building.Level;
                }
            }

            return level;
        }

        private bool CanShowShipyardTab(ColonyState colony)
        {
            return colony != null
                && colony.Presence == ColonyPresence.Colony
                && _shipProductionService.EnablesShipConstruction(colony);
        }

        private static SettlementCommitmentModel Commitment(RunState run, ColonyState colony, bool scanned)
        {
            var ledger = run.Timeline.Resources;
            var isColony = colony != null && colony.Presence == ColonyPresence.Colony;
            var claimed = colony != null;
            var showCard = !isColony;
            var colonyCount = run.Timeline.ColonyCount();
            var colonyCost = SettlementCosts.ColonyCost(colonyCount);
            var escalation = colonyCount > 0
                ? "Next colony · settlement #" + (colonyCount + 1)
                : string.Empty;
            var warning = !scanned && !claimed
                ? "Scan before founding a colony."
                : string.Empty;
            return new SettlementCommitmentModel(
                showCard,
                FormatCost(SettlementCosts.OutpostCost),
                !claimed && ledger.CanAfford(SettlementCosts.OutpostCost),
                FormatCost(colonyCost),
                scanned && !isColony && ledger.CanAfford(colonyCost),
                escalation,
                warning,
                claimed);
        }

        private List<BuildingRowModel> BuildingRows(RunState run, PlanetDefinition planet, ColonyState colony)
        {
            List<BuildingRowModel> rows = new();
            var catalog = _objectDatabase.SettlementCatalog;
            if (catalog == null)
            {
                return rows;
            }

            var ledger = run.Timeline.Resources;
            var isColony = colony != null && colony.Presence == ColonyPresence.Colony;
            var used = UsedFootprint(colony);
            var buildings = catalog.Buildings;
            for (var i = 0; i < buildings.Count; i++)
            {
                var definition = buildings[i];
                if (definition == null || definition.Id.IsEmpty)
                {
                    continue;
                }

                var placed = colony?.FindBuilding(definition.Id);
                var level = placed != null ? placed.Level : 0;
                var hasRoom = used + definition.Footprint <= planet.FootprintBudget;
                rows.Add(new BuildingRowModel(
                    definition.Id,
                    definition.DisplayName,
                    definition.Footprint,
                    level,
                    FormatCost(definition.BuildCost.Amounts),
                    OutputSummary(definition),
                    isColony && placed == null && hasRoom && ledger.CanAfford(definition.BuildCost.Amounts),
                    UpgradeLabel(definition, placed),
                    CanUpgrade(ledger, definition, placed, isColony)));
            }

            return rows;
        }

        private static string OutputSummary(BuildingDefinitionSO definition)
        {
            var outputs = definition.Output;
            if (outputs == null || outputs.Count == 0)
            {
                return string.Empty;
            }

            var text = string.Empty;
            for (var i = 0; i < outputs.Count; i++)
            {
                var entry = outputs[i];
                if (text.Length > 0)
                {
                    text += ", ";
                }

                text += "+" + FormatQuantity(entry.Amount) + " " + entry.Kind;
            }

            return text;
        }

        private static bool CanUpgrade(
            ResourceLedger ledger,
            BuildingDefinitionSO definition,
            ColonyBuildingState placed,
            bool isColony)
        {
            if (!isColony || placed == null || placed.Level >= definition.MaxLevel)
            {
                return false;
            }

            if (!definition.TryGetUpgradeCost(placed.Level, out var cost))
            {
                return false;
            }

            return ledger.CanAfford(cost.Amounts);
        }

        private int UsedFootprint(ColonyState colony)
        {
            if (colony == null)
            {
                return 0;
            }

            var catalog = _objectDatabase.SettlementCatalog;
            var used = 0;
            foreach (var placed in colony.Buildings.Values)
            {
                if (catalog != null && catalog.TryGetBuilding(placed.BuildingId, out var definition))
                {
                    used += definition.Footprint;
                }
            }

            return used;
        }

        private static string UpgradeLabel(BuildingDefinitionSO definition, ColonyBuildingState placed)
        {
            if (placed != null && definition.TryGetUpgradeCost(placed.Level, out var cost))
            {
                return "Upgrade · " + FormatCost(cost.Amounts);
            }

            return "Upgrade";
        }

        private static IReadOnlyList<string> DepositChips(PlanetDefinition planet)
        {
            List<string> chips = new();
            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                if (planet.HasDeposit(kinds[i]))
                {
                    chips.Add(kinds[i].ToString());
                }
            }

            return chips;
        }

        private static string DepositText(PlanetDefinition planet)
        {
            var chips = DepositChips(planet);
            if (chips.Count == 0)
            {
                return "none";
            }

            var text = chips[0];
            for (var i = 1; i < chips.Count; i++)
            {
                text += ", " + chips[i];
            }

            return text;
        }

        private static string FormatCost(IReadOnlyList<ResourceAmount> amounts)
        {
            if (amounts == null || amounts.Count == 0)
            {
                return "free";
            }

            var text = string.Empty;
            for (var i = 0; i < amounts.Count; i++)
            {
                if (text.Length > 0)
                {
                    text += ", ";
                }

                text += amounts[i].Amount + " " + amounts[i].Kind;
            }

            return text;
        }

        private static string FormatQuantity(float amount)
        {
            var nearest = MathF.Round(amount);
            if (MathF.Abs(amount - nearest) < 0.001f)
            {
                return ((int)nearest).ToString();
            }

            return amount.ToString("0.#");
        }

        private static ResourceInventory Stockpile(ResourceLedger ledger)
        {
            var inventory = new ResourceInventory();
            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                inventory.Add(kinds[i], ledger.Get(kinds[i]));
            }

            return inventory;
        }

        private static bool IsScanned(RunState run, ContentId planetId)
        {
            return run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge)
                && knowledge.HasScannedPlanet(planetId);
        }
    }
}
