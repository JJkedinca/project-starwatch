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
    /// Selected-node card: title, summary, and whether travel can start.
    /// </summary>
    internal readonly struct StarMapCardModel
    {
        public static StarMapCardModel Empty { get; } = new(false, string.Empty, string.Empty, false, false, 0f);

        public bool HasSelection { get; }

        public string Title { get; }

        public string Summary { get; }

        public bool CanTravel { get; }

        public bool HasArrival { get; }

        public float ArrivalSeconds { get; }

        public StarMapCardModel(
            bool hasSelection,
            string title,
            string summary,
            bool canTravel,
            bool hasArrival,
            float arrivalSeconds)
        {
            HasSelection = hasSelection;
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            CanTravel = canTravel;
            HasArrival = hasArrival;
            ArrivalSeconds = arrivalSeconds;
        }
    }

    /// <summary>
    /// Star map snapshot: nodes, known routes, and the selected-node card.
    /// </summary>
    internal sealed class StarMapModel
    {
        public static StarMapModel Empty { get; } = new(
            false,
            Array.Empty<StarMapNodeModel>(),
            Array.Empty<MapRouteSegment>(),
            false,
            string.Empty,
            string.Empty,
            false,
            false,
            0f,
            Array.Empty<FleetChoiceModel>());

        public bool HasRun { get; }

        public IReadOnlyList<StarMapNodeModel> Nodes { get; }

        public IReadOnlyList<MapRouteSegment> Routes { get; }

        public bool HasSelection { get; }

        public string Title { get; }

        public string Summary { get; }

        public bool CanTravel { get; }

        public bool HasArrival { get; }

        public float ArrivalSeconds { get; }

        public IReadOnlyList<FleetChoiceModel> Fleets { get; }

        public StarMapModel(
            bool hasRun,
            IReadOnlyList<StarMapNodeModel> nodes,
            IReadOnlyList<MapRouteSegment> routes,
            bool hasSelection,
            string title,
            string summary,
            bool canTravel,
            bool hasArrival,
            float arrivalSeconds,
            IReadOnlyList<FleetChoiceModel> fleets)
        {
            HasRun = hasRun;
            Nodes = nodes ?? Array.Empty<StarMapNodeModel>();
            Routes = routes ?? Array.Empty<MapRouteSegment>();
            HasSelection = hasSelection;
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            CanTravel = canTravel;
            HasArrival = hasArrival;
            ArrivalSeconds = arrivalSeconds;
            Fleets = fleets ?? Array.Empty<FleetChoiceModel>();
        }
    }
}
