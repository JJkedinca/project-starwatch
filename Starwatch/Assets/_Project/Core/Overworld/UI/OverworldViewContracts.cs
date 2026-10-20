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

        event Action<EntityId> FleetSelected;

        event Action<ContentId> TravelRequested;

        void Apply(StarMapModel model);

        /// <summary>
        /// Don't hold the list. It is reused after this call returns.
        /// </summary>
        void ApplyFleets(IReadOnlyList<FleetChoiceModel> fleets);

        /// <summary>
        /// Don't hold the list. It is reused after this call returns.
        /// </summary>
        void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes);

        void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes);

        void ApplyCard(StarMapCardModel card);
    }

    /// <summary>
    /// Open star system, including the map/system toggle and the selected planet.
    /// </summary>
    internal interface IStarSystemView
    {
        event Action BackRequested;

        event Action<ContentId> PlanetSelected;

        event Action<EntityId> FleetSelected;

        event Action<ContentId> ScanRequested;

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
    /// Inspection of the selected fleet. Hidden when no fleet is selected.
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
    }

    /// <summary>
    /// Empire research offers and the active project.
    /// </summary>
    internal interface IResearchView
    {
        event Action<ContentId> StartRequested;

        event Action<ContentId> SwitchConfirmed;

        event Action Opened;

        bool IsOpen { get; }

        void Apply(ResearchPanelModel model);

        void PromptSwitch(ContentId researchId);

        void DismissSwitch();
    }
}
