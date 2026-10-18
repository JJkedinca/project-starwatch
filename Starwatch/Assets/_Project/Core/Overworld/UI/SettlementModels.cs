using System;
using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Overworld.UI
{
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

        public string Development { get; }

        public string Status { get; }

        public SettlementCommitmentModel Commitment { get; }

        public IReadOnlyList<BuildingRowModel> Buildings { get; }

        public SettlementPanelModel(
            bool showPanel,
            ResourceInventory resources,
            PlanetEconomy economy,
            bool showPlanet,
            string development,
            string status,
            SettlementCommitmentModel commitment,
            IReadOnlyList<BuildingRowModel> buildings)
        {
            ShowPanel = showPanel;
            Resources = resources ?? new ResourceInventory();
            Economy = economy;
            ShowPlanet = showPlanet;
            Development = development ?? string.Empty;
            Status = status ?? string.Empty;
            Commitment = commitment;
            Buildings = buildings ?? Array.Empty<BuildingRowModel>();
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
        public string OutpostLabel { get; }

        public bool CanFoundOutpost { get; }

        public string ColonyLabel { get; }

        public bool CanFoundColony { get; }

        public bool CanAbandon { get; }

        public SettlementCommitmentModel(
            string outpostLabel,
            bool canFoundOutpost,
            string colonyLabel,
            bool canFoundColony,
            bool canAbandon)
        {
            OutpostLabel = outpostLabel ?? string.Empty;
            CanFoundOutpost = canFoundOutpost;
            ColonyLabel = colonyLabel ?? string.Empty;
            CanFoundColony = canFoundColony;
            CanAbandon = canAbandon;
        }
    }

    /// <summary>
    /// One catalog building on the selected planet.
    /// </summary>
    internal readonly struct BuildingRowModel
    {
        public ContentId BuildingId { get; }

        public string Label { get; }

        public string PlaceLabel { get; }

        public bool CanPlace { get; }

        public string UpgradeLabel { get; }

        public bool CanUpgrade { get; }

        public BuildingRowModel(
            ContentId buildingId,
            string label,
            string placeLabel,
            bool canPlace,
            string upgradeLabel,
            bool canUpgrade)
        {
            BuildingId = buildingId;
            Label = label ?? string.Empty;
            PlaceLabel = placeLabel ?? string.Empty;
            CanPlace = canPlace;
            UpgradeLabel = upgradeLabel ?? string.Empty;
            CanUpgrade = canUpgrade;
        }
    }
}
