using System;
using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.Services;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Read model for the overworld map: known routes, node and planet rows, and inspection text.
    /// </summary>
    internal sealed class OverworldMapQuery
    {
        private readonly VisibilityService _visibility;
        private readonly TravelService _travel;
        private readonly ObjectDatabaseSO _objectDatabase;

        public OverworldMapQuery(
            VisibilityService visibility,
            TravelService travel,
            ObjectDatabaseSO objectDatabase)
        {
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
        }

        /// <summary>
        /// Known routes drawn on the star map.
        /// </summary>
        internal List<MapRouteSegment> KnownRoutes(RunState run)
        {
            List<MapRouteSegment> segments = new();
            if (run == null || !run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge))
            {
                return segments;
            }

            var graph = run.Universe.MapGraph;
            for (var i = 0; i < graph.Routes.Count; i++)
            {
                var route = graph.Routes[i];
                if (route == null)
                {
                    continue;
                }

                var key = RouteService.MakeRouteKey(route.NodeAId, route.NodeBId);
                if (!knowledge.HasKnownRoute(key))
                {
                    continue;
                }

                if (!graph.TryGetNode(route.NodeAId, out var nodeA)
                    || !graph.TryGetNode(route.NodeBId, out var nodeB))
                {
                    continue;
                }

                segments.Add(new MapRouteSegment(nodeA.MapPosition, nodeB.MapPosition));
            }

            return segments;
        }

        /// <summary>
        /// Presentation for one map node, including the unexplored "?" marker.
        /// </summary>
        internal MapNodePresentationState NodeState(RunState run, MapNode node, ContentId selectedNodeId)
        {
            var visibility = _visibility.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var hasFleet = visibility != MapNodeVisibility.Hidden && NodeHasFleet(run.Timeline, node.Id);
            return new MapNodePresentationState(
                visibility,
                isExplorable: visibility == MapNodeVisibility.Hidden && IsKnownNeighbor(run, node.Id),
                isSelected: node.Id == selectedNodeId,
                hasFleet,
                isFleetTraveling: hasFleet && NodeFleetIsTraveling(run.Timeline, node.Id),
                revealedLabel: visibility == MapNodeVisibility.Hidden ? string.Empty : NodeLabel(node));
        }

        /// <summary>
        /// An explored location can be opened, including one with no planets.
        /// </summary>
        internal bool CanEnter(RunState run, ContentId nodeId)
        {
            if (run == null || nodeId.IsEmpty || !run.Universe.MapGraph.TryGetNode(nodeId, out _))
            {
                return false;
            }

            return run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge)
                && knowledge.HasExploredNode(nodeId);
        }

        /// <summary>
        /// Planets in the location that are no longer hidden.
        /// </summary>
        internal List<ContentId> VisiblePlanets(RunState run, MapNode node)
        {
            List<ContentId> planets = new();
            if (run == null || node == null)
            {
                return planets;
            }

            for (var i = 0; i < node.PlanetIds.Count; i++)
            {
                var planetId = node.PlanetIds[i];
                var visibility = _visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    node.Id,
                    planetId);
                if (visibility == PlanetVisibility.Hidden)
                {
                    continue;
                }

                planets.Add(planetId);
            }

            return planets;
        }

        /// <summary>
        /// Icon state for one planet in the open star system.
        /// </summary>
        internal PlanetPresentationState PlanetState(
            RunState run,
            MapNode node,
            ContentId planetId,
            ContentId selectedPlanetId)
        {
            var visibility = _visibility.GetPlanetVisibility(
                run.Archive,
                ObserverId.Player,
                node.Id,
                planetId);
            return new PlanetPresentationState(
                visibility,
                isSelected: planetId == selectedPlanetId,
                DisplayName(planetId));
        }

        /// <summary>
        /// Inspection title, summary, travel affordance, and selected planet detail.
        /// </summary>
        internal OverworldInspection DescribeSelection(
            RunState run,
            ContentId selectedNodeId,
            ContentId selectedPlanetId,
            bool inStarSystemView)
        {
            if (run == null
                || selectedNodeId.IsEmpty
                || !run.Universe.MapGraph.TryGetNode(selectedNodeId, out var node))
            {
                return OverworldInspection.Empty;
            }

            var visibility = _visibility.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var canTravel = CanTravelTo(run, node.Id);
            if (visibility == MapNodeVisibility.Hidden)
            {
                return new OverworldInspection(
                    "Uncharted space",
                    canTravel
                        ? "Known route. Travel here to enter."
                        : "Hidden until a fleet arrives by a known route.",
                    canTravel,
                    showPlanetDetail: false,
                    default);
            }

            var kindLabel = node.Kind == MapNodeKind.System ? "Star system" : "Non-system node";
            var summary = kindLabel + " · " + node.Id.Value;
            if (visibility == MapNodeVisibility.Present)
            {
                summary += "\nFleet present.";
            }

            if (!inStarSystemView)
            {
                summary += node.PlanetIds.Count > 0
                    ? "\nOpen to inspect planets."
                    : "\nOpen to inspect.";
            }

            var showPlanetDetail = TryPlanetDetail(run, node, selectedPlanetId, inStarSystemView, out var detail);
            return new OverworldInspection(
                NodeLabel(node),
                summary,
                canTravel,
                showPlanetDetail,
                detail);
        }

        /// <summary>
        /// Status line for an open star system, including one with no planets.
        /// </summary>
        internal string StarSystemStatus(int visiblePlanetCount)
        {
            return visiblePlanetCount > 0
                ? "Select a planet to inspect or scan."
                : "No planets in this system.";
        }

        /// <summary>
        /// Display label for a map node.
        /// </summary>
        internal string NodeLabel(MapNode node)
        {
            return node == null || node.Id.IsEmpty ? string.Empty : node.Id.Value;
        }

        /// <summary>
        /// First fleet that can start travel to the destination.
        /// </summary>
        internal bool TryFindFleetForDestination(RunState run, ContentId destinationNodeId, out EntityId fleetId)
        {
            fleetId = EntityId.None;
            if (run == null)
            {
                return false;
            }

            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                if (_travel.CanStartTravel(run, ObserverId.Player, fleets[i].Id, destinationNodeId))
                {
                    fleetId = fleets[i].Id;
                    return true;
                }
            }

            return false;
        }

        private bool TryPlanetDetail(
            RunState run,
            MapNode node,
            ContentId selectedPlanetId,
            bool inStarSystemView,
            out PlanetDetailModel detail)
        {
            detail = default;
            if (!inStarSystemView || selectedPlanetId.IsEmpty)
            {
                return false;
            }

            var planetVisibility = _visibility.GetPlanetVisibility(
                run.Archive,
                ObserverId.Player,
                node.Id,
                selectedPlanetId);
            if (planetVisibility == PlanetVisibility.Hidden)
            {
                return false;
            }

            var displayName = DisplayName(selectedPlanetId);
            var text = "Ready to scan.";
            var canScan = planetVisibility == PlanetVisibility.SystemExplored;
            if (_objectDatabase.TryGetPlanet(selectedPlanetId.Value, out var definition))
            {
                if (planetVisibility == PlanetVisibility.Scanned)
                {
                    text = definition.Summary;
                }
            }
            else if (planetVisibility == PlanetVisibility.Scanned)
            {
                text = "Scanned.";
            }

            detail = new PlanetDetailModel(selectedPlanetId, displayName, text, canScan);
            return true;
        }

        private string DisplayName(ContentId planetId)
        {
            if (_objectDatabase.TryGetPlanet(planetId.Value, out var definition))
            {
                return definition.DisplayName;
            }

            return planetId.Value;
        }

        private bool CanTravelTo(RunState run, ContentId destinationNodeId)
        {
            return TryFindFleetForDestination(run, destinationNodeId, out _);
        }

        private bool IsKnownNeighbor(RunState run, ContentId nodeId)
        {
            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                if (_travel.IsKnownAdjacentDestination(run, ObserverId.Player, fleets[i].LocationSystemId, nodeId))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool NodeHasFleet(TimelineState timeline, ContentId nodeId)
        {
            for (var i = 0; i < timeline.Fleets.Count; i++)
            {
                if (timeline.Fleets[i].LocationSystemId == nodeId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool NodeFleetIsTraveling(TimelineState timeline, ContentId nodeId)
        {
            for (var i = 0; i < timeline.Fleets.Count; i++)
            {
                var fleet = timeline.Fleets[i];
                if (fleet.LocationSystemId == nodeId && timeline.IsFleetTraveling(fleet.Id))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Inspection snapshot for the selected map location and, when open, its planet.
    /// </summary>
    internal readonly struct OverworldInspection
    {
        public static OverworldInspection Empty => default;

        public bool HasSelection { get; }

        public string Title { get; }

        public string Summary { get; }

        public bool CanTravel { get; }

        public bool ShowPlanetDetail { get; }

        public PlanetDetailModel PlanetDetail { get; }

        public OverworldInspection(
            string title,
            string summary,
            bool canTravel,
            bool showPlanetDetail,
            PlanetDetailModel planetDetail)
        {
            HasSelection = true;
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            CanTravel = canTravel;
            ShowPlanetDetail = showPlanetDetail;
            PlanetDetail = planetDetail;
        }
    }
}
