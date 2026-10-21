using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One map node: where it sits, what kind it is, and how it is drawn.
    /// </summary>
    internal readonly struct StarMapNodeModel
    {
        public ContentId NodeId { get; }

        public MapNodeKind Kind { get; }

        public Vector2 MapPosition { get; }

        public MapNodePresentationState State { get; }

        public StarMapNodeModel(
            ContentId nodeId,
            MapNodeKind kind,
            Vector2 mapPosition,
            MapNodePresentationState state)
        {
            NodeId = nodeId;
            Kind = kind;
            MapPosition = mapPosition;
            State = state;
        }
    }

    /// <summary>
    /// How the selected fleet relates to the selected map target.
    /// </summary>
    internal enum StarMapFleetRelation
    {
        NoneSelected = 0,
        AtTarget = 1,
        InTransit = 2,
        Reachable = 3,
        Unreachable = 4,
    }

    /// <summary>
    /// Selected-node target card on the star map.
    /// </summary>
    internal readonly struct StarMapCardModel
    {
        public static StarMapCardModel Empty { get; } = new(
            isShown: true,
            hasSelection: false,
            string.Empty,
            false,
            false,
            0,
            0,
            StarMapFleetRelation.NoneSelected,
            string.Empty,
            0f,
            false);

        /// <summary>
        /// Hidden while a star system is open — the system header already shows that info.
        /// </summary>
        public static StarMapCardModel Hidden { get; } = new(
            isShown: false,
            hasSelection: false,
            string.Empty,
            false,
            false,
            0,
            0,
            StarMapFleetRelation.NoneSelected,
            string.Empty,
            0f,
            false);

        public bool IsShown { get; }

        public bool HasSelection { get; }

        public string Title { get; }

        public bool IsExplored { get; }

        public bool HasFleetPresent { get; }

        public int BodyCount { get; }

        public int ScannedBodyCount { get; }

        public StarMapFleetRelation FleetRelation { get; }

        public string FleetName { get; }

        public float RelationSeconds { get; }

        public bool ShowTravelHint { get; }

        /// <summary>
        /// Reachable target: trip seconds match StartTravel.
        /// </summary>
        public bool HasArrival => FleetRelation == StarMapFleetRelation.Reachable;

        public float ArrivalSeconds => HasArrival ? RelationSeconds : 0f;

        public StarMapCardModel(
            bool hasSelection,
            string title,
            bool isExplored,
            bool hasFleetPresent,
            int bodyCount,
            int scannedBodyCount,
            StarMapFleetRelation fleetRelation,
            string fleetName,
            float relationSeconds,
            bool showTravelHint)
            : this(
                isShown: true,
                hasSelection,
                title,
                isExplored,
                hasFleetPresent,
                bodyCount,
                scannedBodyCount,
                fleetRelation,
                fleetName,
                relationSeconds,
                showTravelHint)
        {
        }

        private StarMapCardModel(
            bool isShown,
            bool hasSelection,
            string title,
            bool isExplored,
            bool hasFleetPresent,
            int bodyCount,
            int scannedBodyCount,
            StarMapFleetRelation fleetRelation,
            string fleetName,
            float relationSeconds,
            bool showTravelHint)
        {
            IsShown = isShown;
            HasSelection = hasSelection;
            Title = title ?? string.Empty;
            IsExplored = isExplored;
            HasFleetPresent = hasFleetPresent;
            BodyCount = bodyCount < 0 ? 0 : bodyCount;
            ScannedBodyCount = scannedBodyCount < 0 ? 0 : scannedBodyCount;
            FleetRelation = fleetRelation;
            FleetName = fleetName ?? string.Empty;
            RelationSeconds = relationSeconds < 0f ? 0f : relationSeconds;
            ShowTravelHint = showTravelHint;
        }
    }

    /// <summary>
    /// Hover tooltip over a map node while a fleet is selected.
    /// </summary>
    internal readonly struct StarMapHoverTooltipModel
    {
        public static StarMapHoverTooltipModel Hidden { get; } = new(false, string.Empty, 0f, false);

        public bool IsShown { get; }

        public string TargetName { get; }

        public float Seconds { get; }

        public bool Reachable { get; }

        public StarMapHoverTooltipModel(bool isShown, string targetName, float seconds, bool reachable)
        {
            IsShown = isShown;
            TargetName = targetName ?? string.Empty;
            Seconds = seconds < 0f ? 0f : seconds;
            Reachable = reachable;
        }
    }

    /// <summary>
    /// Star map snapshot: nodes, routes, and the selected-node card.
    /// </summary>
    internal sealed class StarMapModel
    {
        public static StarMapModel Empty { get; } = new(
            false,
            Array.Empty<StarMapNodeModel>(),
            Array.Empty<MapRouteSegment>(),
            StarMapCardModel.Empty,
            Array.Empty<FleetRowModel>(),
            Array.Empty<FleetMarkerModel>());

        public bool HasRun { get; }

        public IReadOnlyList<StarMapNodeModel> Nodes { get; }

        public IReadOnlyList<MapRouteSegment> Routes { get; }

        public StarMapCardModel Card { get; }

        public IReadOnlyList<FleetRowModel> Fleets { get; }

        public IReadOnlyList<FleetMarkerModel> FleetMarkers { get; }

        public bool HasSelection => Card.HasSelection;

        public string Title => Card.Title;

        public bool HasArrival => Card.HasArrival;

        public float ArrivalSeconds => Card.ArrivalSeconds;

        public StarMapModel(
            bool hasRun,
            IReadOnlyList<StarMapNodeModel> nodes,
            IReadOnlyList<MapRouteSegment> routes,
            StarMapCardModel card,
            IReadOnlyList<FleetRowModel> fleets,
            IReadOnlyList<FleetMarkerModel> fleetMarkers)
        {
            HasRun = hasRun;
            Nodes = nodes ?? Array.Empty<StarMapNodeModel>();
            Routes = routes ?? Array.Empty<MapRouteSegment>();
            Card = card;
            Fleets = fleets ?? Array.Empty<FleetRowModel>();
            FleetMarkers = fleetMarkers ?? Array.Empty<FleetMarkerModel>();
        }
    }
}
