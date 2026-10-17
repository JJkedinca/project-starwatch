using System;
using System.Collections.Generic;
using System.Text;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Settlement;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Read model for the settlement panel on an open star system.
    /// Base stats and current production come from <see cref="ProductionService"/>.
    /// </summary>
    internal sealed class SettlementQuery
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly ProductionService _production;

        public SettlementQuery(ObjectDatabaseSO objectDatabase, ProductionService production)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _production = production ?? throw new ArgumentNullException(nameof(production));
        }

        /// <summary>
        /// Stockpile for the empire, and the selected planet's resources, development, commitment, and buildings.
        /// </summary>
        internal SettlementPanelModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null || selection == null || !selection.InStarSystem)
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
                    string.Empty,
                    string.Empty,
                    default,
                    Array.Empty<BuildingRowModel>());
            }

            run.Timeline.TryGetColony(planetId, out var colony);
            var scanned = IsScanned(run, planetId);
            return new SettlementPanelModel(
                showPanel: true,
                resources,
                scanned ? DescribeEconomy(planet, colony) : default,
                showPlanet: true,
                scanned ? DevelopmentText(planet, colony) : "Scan to reveal development.",
                StatusText(colony),
                Commitment(run, colony, scanned),
                BuildingRows(run, planet, colony));
        }

        private PlanetEconomy DescribeEconomy(PlanetDefinition planet, ColonyState colony)
        {
            var quote = _production.Quote(colony, planet);
            return new PlanetEconomy(_production.ModifiedBase(planet), quote.Output, quote.Upkeep);
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

        private static SettlementCommitmentModel Commitment(RunState run, ColonyState colony, bool scanned)
        {
            var ledger = run.Timeline.Resources;
            var isColony = colony != null && colony.Presence == ColonyPresence.Colony;
            var claimed = colony != null;
            var colonyCost = SettlementCosts.ColonyCost(run.Timeline.ColonyCount());
            return new SettlementCommitmentModel(
                "Outpost · " + FormatCost(SettlementCosts.OutpostCost),
                !claimed && ledger.CanAfford(SettlementCosts.OutpostCost),
                "Colony · " + FormatCost(colonyCost),
                scanned && !isColony && ledger.CanAfford(colonyCost),
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
                var label = definition.DisplayName + " · footprint " + definition.Footprint;
                if (placed != null)
                {
                    label += " · Lv " + placed.Level;
                }

                var hasRoom = used + definition.Footprint <= planet.FootprintBudget;
                rows.Add(new BuildingRowModel(
                    definition.Id,
                    label,
                    "Place · " + FormatCost(definition.BuildCost.Amounts),
                    isColony && placed == null && hasRoom && ledger.CanAfford(definition.BuildCost.Amounts),
                    UpgradeLabel(definition, placed),
                    CanUpgrade(ledger, definition, placed, isColony)));
            }

            return rows;
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

        private static string DepositText(PlanetDefinition planet)
        {
            var text = string.Empty;
            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                if (!planet.HasDeposit(kinds[i]))
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text += ", ";
                }

                text += kinds[i].ToString();
            }

            return text.Length == 0 ? "none" : text;
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
