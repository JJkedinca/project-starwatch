using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// How far the empire has committed to the selected planet.
    /// </summary>
    internal enum SettlementPresenceKind
    {
        None = 0,
        Outpost = 1,
        Colony = 2,
    }

    /// <summary>
    /// Fixed dossier header for the selected planet.
    /// </summary>
    internal readonly struct SettlementHeaderModel
    {
        public ContentId PlanetId { get; }

        public string PlanetName { get; }

        public bool IsScanned { get; }

        public bool CanScan { get; }

        public SettlementPresenceKind Presence { get; }

        public int Level { get; }

        public int FootprintUsed { get; }

        public int FootprintTotal { get; }

        public IReadOnlyList<string> Chips { get; }

        public bool CanAbandon { get; }

        public SettlementHeaderModel(
            ContentId planetId,
            string planetName,
            bool isScanned,
            bool canScan,
            SettlementPresenceKind presence,
            int level,
            int footprintUsed,
            int footprintTotal,
            IReadOnlyList<string> chips,
            bool canAbandon)
        {
            PlanetId = planetId;
            PlanetName = planetName ?? string.Empty;
            IsScanned = isScanned;
            CanScan = canScan;
            Presence = presence;
            Level = level < 0 ? 0 : level;
            FootprintUsed = footprintUsed < 0 ? 0 : footprintUsed;
            FootprintTotal = footprintTotal < 0 ? 0 : footprintTotal;
            Chips = chips ?? Array.Empty<string>();
            CanAbandon = canAbandon;
        }
    }

    /// <summary>
    /// Settlement panel snapshot. Hidden while the star map is showing.
    /// </summary>
    internal readonly struct SettlementPanelModel
    {
        public static SettlementPanelModel Hidden => default;

        public bool ShowPanel { get; }

        public ResourceInventory Resources { get; }

        /// <summary>
        /// Planet base and current production. Hidden until the planet is scanned.
        /// </summary>
        public PlanetEconomy Economy { get; }

        public bool ShowPlanet { get; }

        public SettlementHeaderModel Header { get; }

        /// <summary>
        /// Legacy multiline development copy for tests and fallbacks.
        /// </summary>
        public string Development { get; }

        /// <summary>
        /// Legacy status label (Unclaimed / Outpost / Colony).
        /// </summary>
        public string Status { get; }

        public SettlementCommitmentModel Commitment { get; }

        public IReadOnlyList<BuildingRowModel> Buildings { get; }

        public bool ShowShipyardTab { get; }

        public SettlementPanelModel(
            bool showPanel,
            ResourceInventory resources,
            PlanetEconomy economy,
            bool showPlanet,
            SettlementHeaderModel header,
            string development,
            string status,
            SettlementCommitmentModel commitment,
            IReadOnlyList<BuildingRowModel> buildings,
            bool showShipyardTab)
        {
            ShowPanel = showPanel;
            Resources = resources ?? new ResourceInventory();
            Economy = economy;
            ShowPlanet = showPlanet;
            Header = header;
            Development = development ?? string.Empty;
            Status = status ?? string.Empty;
            Commitment = commitment;
            Buildings = buildings ?? Array.Empty<BuildingRowModel>();
            ShowShipyardTab = showShipyardTab;
        }
    }

    /// <summary>
    /// Scanned planet yield. Base is level-1 output with planet modifiers, deposits, and research multipliers applied.
    /// Production is the placed buildings.
    /// </summary>
    internal readonly struct PlanetEconomy
    {
        public bool IsRevealed { get; }

        public ResourceInventory Base { get; }

        public ResourceInventory Production { get; }

        public ResourceInventory Upkeep { get; }

        public PlanetEconomy(ResourceInventory planetBase, ResourceInventory production, ResourceInventory upkeep)
        {
            IsRevealed = true;
            Base = planetBase ?? new ResourceInventory();
            Production = production ?? new ResourceInventory();
            Upkeep = upkeep ?? new ResourceInventory();
        }

        public float Rate(ResourceKind kind)
        {
            return (Production?.Get(kind) ?? 0f) - (Upkeep?.Get(kind) ?? 0f);
        }
    }

    /// <summary>
    /// Found and abandon affordances for the selected planet.
    /// </summary>
    internal readonly struct SettlementCommitmentModel
    {
        public bool ShowCard { get; }

        public string OutpostCostLabel { get; }

        public bool CanFoundOutpost { get; }

        public string ColonyCostLabel { get; }

        public bool CanFoundColony { get; }

        public string EscalationNote { get; }

        public string Warning { get; }

        public bool CanAbandon { get; }

        /// <summary>
        /// Compatibility: same as CanFoundOutpost for older tests.
        /// </summary>
        public string OutpostLabel => OutpostCostLabel;

        public string ColonyLabel => ColonyCostLabel;

        public SettlementCommitmentModel(
            bool showCard,
            string outpostCostLabel,
            bool canFoundOutpost,
            string colonyCostLabel,
            bool canFoundColony,
            string escalationNote,
            string warning,
            bool canAbandon)
        {
            ShowCard = showCard;
            OutpostCostLabel = outpostCostLabel ?? string.Empty;
            CanFoundOutpost = canFoundOutpost;
            ColonyCostLabel = colonyCostLabel ?? string.Empty;
            CanFoundColony = canFoundColony;
            EscalationNote = escalationNote ?? string.Empty;
            Warning = warning ?? string.Empty;
            CanAbandon = canAbandon;
        }
    }

    /// <summary>
    /// One catalog building on the selected planet.
    /// </summary>
    internal readonly struct BuildingRowModel
    {
        public ContentId BuildingId { get; }

        public string Name { get; }

        public int Footprint { get; }

        public int Level { get; }

        public string CostLabel { get; }

        public string OutputLabel { get; }

        public bool CanPlace { get; }

        public string UpgradeLabel { get; }

        public bool CanUpgrade { get; }

        public BuildingRowModel(
            ContentId buildingId,
            string name,
            int footprint,
            int level,
            string costLabel,
            string outputLabel,
            bool canPlace,
            string upgradeLabel,
            bool canUpgrade)
        {
            BuildingId = buildingId;
            Name = name ?? string.Empty;
            Footprint = footprint < 0 ? 0 : footprint;
            Level = level < 0 ? 0 : level;
            CostLabel = costLabel ?? string.Empty;
            OutputLabel = outputLabel ?? string.Empty;
            CanPlace = canPlace;
            UpgradeLabel = upgradeLabel ?? string.Empty;
            CanUpgrade = canUpgrade;
        }
    }
}
