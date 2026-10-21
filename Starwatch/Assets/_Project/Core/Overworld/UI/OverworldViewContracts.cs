using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Star map surface and the selected-node card.
    /// </summary>
    internal interface IStarMapView
    {
        event Action<ContentId> NodeSelected;

        event Action<ContentId> TravelRequested;

        /// <summary>
        /// Pointer entered a node, or empty when the pointer left.
        /// </summary>
        event Action<ContentId> NodeHovered;

        void Apply(StarMapModel model);

        /// <summary>
        /// Don't hold the list. It is reused after this call returns.
        /// </summary>
        void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes);

        void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes);

        void ApplyCard(StarMapCardModel card);

        void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip);

        void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers);
    }

    /// <summary>
    /// Open star system scene, including the map/system toggle.
    /// </summary>
    internal interface IStarSystemView
    {
        event Action BackRequested;

        event Action<ContentId> PlanetSelected;

        event Action<EntityId> FleetSelected;

        void Apply(StarSystemModel model);
    }

    /// <summary>
    /// Settlement panel shown while a star system is open.
    /// </summary>
    internal interface ISettlementView
    {
        event Action FoundOutpostRequested;

        event Action FoundColonyRequested;

        event Action AbandonRequested;

        event Action ScanRequested;

        event Action<ContentId> PlaceRequested;

        event Action<ContentId> UpgradeRequested;

        void Apply(SettlementPanelModel model);
    }

    /// <summary>
    /// Shipyard section on the settlement panel. Hidden when the colony cannot build ships.
    /// </summary>
    internal interface IShipyardView
    {
        event Action<ContentId> QueueRequested;

        event Action<int> CancelRequested;

        void Apply(ShipyardModel model);
    }

    /// <summary>
    /// Left-column fleet roster. Visible during a run; hidden when there is no run.
    /// </summary>
    internal interface IFleetRosterView
    {
        event Action<EntityId> FleetSelected;

        /// <summary>
        /// Pass null to hide. An empty list shows the empty roster during a run.
        /// </summary>
        void Apply(IReadOnlyList<FleetRowModel> fleets);
    }

    /// <summary>
    /// Selected-fleet summary bar at the bottom of the center column.
    /// </summary>
    internal interface IFleetBarView
    {
        event Action DetailsRequested;

        event Action CloseRequested;

        void Apply(FleetPanelModel model);
    }

    /// <summary>
    /// Fleet details in the right column. Opened from the fleet bar.
    /// </summary>
    internal interface IFleetPanelView
    {
        event Action CloseRequested;

        event Action<ContentId, int> SplitRequested;

        event Action<EntityId> MergeRequested;

        void Apply(FleetPanelModel model);
    }

    /// <summary>
    /// Empire stockpile. Visible on the star map and inside a star system.
    /// </summary>
    internal interface IStockpileView
    {
        void Apply(StockpileModel model);

        /// <summary>
        /// Refreshes only the research segment. Stock amounts stay on the economy publish window.
        /// </summary>
        void ApplyResearchProgress(StockpileModel model);
    }

    /// <summary>
    /// Empire research offers and the active project.
    /// </summary>
    internal interface IResearchView
    {
        event Action<ContentId> StartRequested;

        event Action<ContentId> SwitchConfirmed;

        /// <summary>
        /// Stockpile entry or close button asked to toggle the right-column research panel.
        /// </summary>
        event Action ToggleRequested;

        bool IsOpen { get; }

        void SetOpen(bool open);

        void Apply(ResearchPanelModel model);

        void PromptSwitch(ContentId researchId);

        void DismissSwitch();
    }
}
