using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Star map surface and the selected-node card.
    /// </summary>
    internal interface IStarMapView
    {
        event Action<ContentId> NodeSelected;

        event Action TravelRequested;

        void Apply(StarMapModel model);

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
    /// Empire stockpile. Visible on the star map and inside a star system.
    /// </summary>
    internal interface IStockpileView
    {
        void Apply(ResourceInventory stockpile);
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
