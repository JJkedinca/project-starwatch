using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Services;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Read model for the star map and the selected-node card.
    /// </summary>
    internal sealed class StarMapQuery
    {
        private readonly VisibilityService _visibilityService;
        private readonly TravelService _travelService;
        private readonly FleetListQuery _fleetListQuery;
        private readonly ObjectDatabaseSO _database;
        private readonly SimulationClock _clock;

        public StarMapQuery(
            VisibilityService visibilityService,
            TravelService travelService,
            FleetListQuery fleetListQuery,
            ObjectDatabaseSO database,
            SimulationClock clock)
        {
            _visibilityService = visibilityService ?? throw new ArgumentNullException(nameof(visibilityService));
            _travelService = travelService ?? throw new ArgumentNullException(nameof(travelService));
            _fleetListQuery = fleetListQuery ?? throw new ArgumentNullException(nameof(fleetListQuery));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Nodes, routes, and the card for the selected node.
        /// </summary>
        internal StarMapModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null)
            {
                return StarMapModel.Empty;
            }

            var selectedNodeId = selection == null ? default : selection.SelectedNodeId;
            var selectedFleetId = selection == null ? EntityId.None : selection.SelectedFleetId;
            List<StarMapNodeModel> nodes = new();
            foreach (var node in run.Universe.MapGraph.Nodes)
            {
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                nodes.Add(new StarMapNodeModel(
                    node.Id,
                    node.Kind,
                    node.MapPosition,
                    NodeState(run, node, selectedNodeId)));
            }

            return new StarMapModel(
                true,
                nodes,
                DescribeRoutes(run, selection, default),
                DescribeCard(run, selection),
                _fleetListQuery.All(run, selectedFleetId),
                DescribeFleetMarkers(run, selection));
        }

        /// <summary>
        /// Parked arrows (idle fleets per node) and one transit arrow per traveling fleet.
        /// </summary>
        internal List<FleetMarkerModel> DescribeFleetMarkers(RunState run, OverworldSelection selection)
        {
            List<FleetMarkerModel> markers = new();
            if (run == null)
            {
                return markers;
            }

            var selectedFleetId = selection == null ? EntityId.None : selection.SelectedFleetId;
            var fleets = run.Timeline.Fleets;
            var graph = run.Universe.MapGraph;

            var travels = fleets.ActiveTravels;
            for (var i = 0; i < travels.Count; i++)
            {
                var travel = travels[i];
                if (!graph.TryGetNode(travel.FromNodeId, out var fromNode)
                    || !graph.TryGetNode(travel.ToNodeId, out var toNode))
                {
                    continue;
                }

                var total = (float)(travel.ArriveTime.Seconds - travel.DepartTime.Seconds);
                if (total < 0f)
                {
                    total = 0f;
                }

                markers.Add(new FleetMarkerModel(
                    FleetMarkerKind.Transit,
                    travel.FleetId,
                    travel.FromNodeId,
                    travel.ToNodeId,
                    fromNode.MapPosition,
                    toNode.MapPosition,
                    count: 1,
                    isSelected: travel.FleetId == selectedFleetId,
                    OverworldNames.RemainingSeconds(_clock, travel.ArriveTime),
                    total));
            }

            foreach (var node in graph.Nodes)
            {
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                var visibility = _visibilityService.GetNodeVisibility(
                    run.Archive,
                    run.Timeline,
                    ObserverId.Player,
                    node.Id);
                if (visibility == MapNodeVisibility.Hidden)
                {
                    continue;
                }

                var atNode = fleets.FleetsAt(node.Id);
                var idleCount = 0;
                var selectedHere = false;
                for (var i = 0; i < atNode.Count; i++)
                {
                    var fleetId = atNode[i];
                    if (fleets.IsFleetTraveling(fleetId))
                    {
                        continue;
                    }

                    idleCount++;
                    if (fleetId == selectedFleetId)
                    {
                        selectedHere = true;
                    }
                }

                if (idleCount <= 0)
                {
                    continue;
                }

                markers.Add(new FleetMarkerModel(
                    FleetMarkerKind.Parked,
                    EntityId.None,
                    node.Id,
                    default,
                    node.MapPosition,
                    node.MapPosition,
                    idleCount,
                    selectedHere,
                    remainingSeconds: 0f,
                    totalSeconds: 0f));
            }

            return markers;
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
        /// Known routes plus in-transit legs, and an optional planned hover leg.
        /// </summary>
        internal List<MapRouteSegment> DescribeRoutes(
            RunState run,
            OverworldSelection selection,
            ContentId hoverNodeId)
        {
            List<MapRouteSegment> segments = new();
            if (run == null)
            {
                return segments;
            }

            Dictionary<ContentId, MapRouteSegment> byKey = new();
            AddKnownRoutes(run, byKey);
            AddInTransitRoutes(run, byKey);

            if (selection != null
                && !selection.SelectedFleetId.IsNone
                && !hoverNodeId.IsEmpty)
            {
                var preview = TravelPreview(run, selection.SelectedFleetId, hoverNodeId);
                if (preview.Reachable)
                {
                    TryPutRoute(
                        byKey,
                        preview.FromNodeId,
                        preview.ToNodeId,
                        MapRouteState.Planned,
                        run.Universe.MapGraph);
                }
            }

            foreach (var pair in byKey)
            {
                segments.Add(pair.Value);
            }

            return segments;
        }

        /// <summary>
        /// Known routes only (no transit or planned overlay).
        /// </summary>
        internal List<MapRouteSegment> KnownRoutes(RunState run)
        {
            Dictionary<ContentId, MapRouteSegment> byKey = new();
            AddKnownRoutes(run, byKey);
            List<MapRouteSegment> segments = new();
            foreach (var pair in byKey)
            {
                segments.Add(pair.Value);
            }

            return segments;
        }

        /// <summary>
        /// Preview of sending <paramref name="fleetId"/> to <paramref name="destinationNodeId"/>.
        /// </summary>
        internal TravelPreviewModel TravelPreview(RunState run, EntityId fleetId, ContentId destinationNodeId)
        {
            if (run == null
                || fleetId.IsNone
                || destinationNodeId.IsEmpty
                || !run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet))
            {
                return TravelPreviewModel.None;
            }

            if (!_travelService.TryGetTripSeconds(
                    run,
                    ObserverId.Player,
                    fleetId,
                    destinationNodeId,
                    out var seconds))
            {
                return TravelPreviewModel.None;
            }

            return new TravelPreviewModel(true, seconds, fleet.LocationSystemId, destinationNodeId);
        }

        internal MapNodePresentationState NodeState(RunState run, MapNode node, ContentId selectedNodeId)
        {
            var visibility = _visibilityService.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, node.Id);
            var hasFleet = visibility != MapNodeVisibility.Hidden && run.Timeline.Fleets.HasFleetAt(node.Id);
            return new MapNodePresentationState(
                visibility,
                isExplorable: visibility == MapNodeVisibility.Hidden && IsKnownNeighbor(run, node.Id),
                isSelected: node.Id == selectedNodeId,
                hasFleet,
                isFleetTraveling: hasFleet && run.Timeline.Fleets.HasTravelingFleetAt(node.Id),
                revealedLabel: visibility == MapNodeVisibility.Hidden ? string.Empty : NodeLabel(node));
        }

        internal StarMapCardModel DescribeCard(RunState run, OverworldSelection selection)
        {
            if (run == null || selection == null)
            {
                return StarMapCardModel.Empty;
            }

            // Fleet details and research take the right column; the system header
            // already covers target info while a star system is open.
            if (selection.RightColumnFocus != RightColumnFocus.Target || selection.InStarSystem)
            {
                return StarMapCardModel.Hidden;
            }

            if (selection.SelectedNodeId.IsEmpty)
            {
                return StarMapCardModel.Empty;
            }

            if (!run.Universe.MapGraph.TryGetNode(selection.SelectedNodeId, out var node))
            {
                return StarMapCardModel.Empty;
            }

            var visibility = _visibilityService.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var explored = visibility != MapNodeVisibility.Hidden;
            var fleetPresent = explored && run.Timeline.Fleets.HasFleetAt(node.Id);
            var bodyCount = node.PlanetIds.Count;
            var scanned = 0;
            if (explored && run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge))
            {
                for (var i = 0; i < node.PlanetIds.Count; i++)
                {
                    if (knowledge.HasScannedPlanet(node.PlanetIds[i]))
                    {
                        scanned++;
                    }
                }
            }

            DescribeFleetRelation(
                run,
                selection.SelectedFleetId,
                node.Id,
                out var relation,
                out var fleetName,
                out var relationSeconds);
            var title = explored ? NodeLabel(node) : "Uncharted space";
            return new StarMapCardModel(
                true,
                node.Id,
                title,
                explored,
                fleetPresent,
                bodyCount,
                scanned,
                relation,
                fleetName,
                relationSeconds,
                relation == StarMapFleetRelation.Reachable);
        }

        internal StarMapHoverTooltipModel DescribeHoverTooltip(
            RunState run,
            OverworldSelection selection,
            ContentId hoverNodeId)
        {
            if (run == null
                || selection == null
                || selection.SelectedFleetId.IsNone
                || hoverNodeId.IsEmpty
                || !run.Universe.MapGraph.TryGetNode(hoverNodeId, out var node))
            {
                return StarMapHoverTooltipModel.Hidden;
            }

            var preview = TravelPreview(run, selection.SelectedFleetId, hoverNodeId);
            var visibility = _visibilityService.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var name = visibility == MapNodeVisibility.Hidden ? "Uncharted space" : NodeLabel(node);
            return new StarMapHoverTooltipModel(true, name, preview.Seconds, preview.Reachable);
        }

        internal void AppendNode(
            RunState run,
            ContentId nodeId,
            ContentId selectedNodeId,
            List<StarMapNodeModel> destination)
        {
            if (run == null
                || destination == null
                || nodeId.IsEmpty
                || ContainsNode(destination, nodeId)
                || !run.Universe.MapGraph.TryGetNode(nodeId, out var node))
            {
                return;
            }

            destination.Add(new StarMapNodeModel(
                node.Id,
                node.Kind,
                node.MapPosition,
                NodeState(run, node, selectedNodeId)));
        }

        internal void AppendNodeAndNeighbors(
            RunState run,
            ContentId nodeId,
            ContentId selectedNodeId,
            List<StarMapNodeModel> destination)
        {
            AppendNode(run, nodeId, selectedNodeId, destination);
            if (run == null || nodeId.IsEmpty)
            {
                return;
            }

            var neighbors = run.Universe.MapGraph.GetNeighbors(nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                AppendNode(run, neighbors[i], selectedNodeId, destination);
            }
        }

        private void DescribeFleetRelation(
            RunState run,
            EntityId selectedFleetId,
            ContentId targetNodeId,
            out StarMapFleetRelation relation,
            out string fleetName,
            out float relationSeconds)
        {
            relation = StarMapFleetRelation.NoneSelected;
            fleetName = string.Empty;
            relationSeconds = 0f;
            if (selectedFleetId.IsNone || !run.Timeline.Fleets.TryGetFleet(selectedFleetId, out var fleet))
            {
                return;
            }

            fleetName = OverworldNames.FleetTitle(_database, fleet);
            if (run.Timeline.Fleets.IsFleetTraveling(selectedFleetId))
            {
                relation = StarMapFleetRelation.InTransit;
                return;
            }

            if (fleet.LocationSystemId == targetNodeId)
            {
                relation = StarMapFleetRelation.AtTarget;
                return;
            }

            if (_travelService.TryGetTripSeconds(
                    run,
                    ObserverId.Player,
                    selectedFleetId,
                    targetNodeId,
                    out relationSeconds))
            {
                relation = StarMapFleetRelation.Reachable;
                return;
            }

            relation = StarMapFleetRelation.Unreachable;
        }

        private void AddKnownRoutes(RunState run, Dictionary<ContentId, MapRouteSegment> byKey)
        {
            var graph = run.Universe.MapGraph;
            for (var i = 0; i < graph.Routes.Count; i++)
            {
                var route = graph.Routes[i];
                if (route == null)
                {
                    continue;
                }

                var key = RouteService.MakeRouteKey(route.NodeAId, route.NodeBId);
                if (!_visibilityService.IsRouteVisibleOnMap(run.Archive, ObserverId.Player, key))
                {
                    continue;
                }

                TryPutRoute(byKey, route.NodeAId, route.NodeBId, MapRouteState.Known, graph);
            }
        }

        private static void AddInTransitRoutes(RunState run, Dictionary<ContentId, MapRouteSegment> byKey)
        {
            var travels = run.Timeline.Fleets.ActiveTravels;
            var graph = run.Universe.MapGraph;
            for (var i = 0; i < travels.Count; i++)
            {
                var travel = travels[i];
                TryPutRoute(byKey, travel.FromNodeId, travel.ToNodeId, MapRouteState.InTransit, graph);
            }
        }

        private static void TryPutRoute(
            Dictionary<ContentId, MapRouteSegment> byKey,
            ContentId nodeAId,
            ContentId nodeBId,
            MapRouteState state,
            MapGraph graph)
        {
            if (nodeAId.IsEmpty
                || nodeBId.IsEmpty
                || !graph.TryGetNode(nodeAId, out var nodeA)
                || !graph.TryGetNode(nodeBId, out var nodeB))
            {
                return;
            }

            var key = RouteService.MakeRouteKey(nodeAId, nodeBId);
            if (byKey.TryGetValue(key, out var existing) && Priority(existing.State) > Priority(state))
            {
                return;
            }

            byKey[key] = new MapRouteSegment(
                nodeAId,
                nodeBId,
                nodeA.MapPosition,
                nodeB.MapPosition,
                state);
        }

        private static int Priority(MapRouteState state)
        {
            switch (state)
            {
                case MapRouteState.Planned:
                    return 3;
                case MapRouteState.InTransit:
                    return 2;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// True when the node is still Hidden but the player has a known route into it
        /// from an explored system. Persists after the fleet leaves that edge — discovery
        /// lives on map knowledge, not current fleet position.
        /// </summary>
        private bool IsKnownNeighbor(RunState run, ContentId nodeId)
        {
            if (run == null
                || nodeId.IsEmpty
                || !run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge)
                || knowledge.HasExploredNode(nodeId))
            {
                return false;
            }

            var neighbors = run.Universe.MapGraph.GetNeighbors(nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                var neighborId = neighbors[i];
                if (!knowledge.HasExploredNode(neighborId))
                {
                    continue;
                }

                if (knowledge.HasKnownRoute(RouteService.MakeRouteKey(neighborId, nodeId)))
                {
                    return true;
                }
            }

            return false;
        }

        private string NodeLabel(MapNode node)
        {
            if (node == null || node.Id.IsEmpty)
            {
                return string.Empty;
            }

            return OverworldNames.StarSystem(_database, node.Id);
        }

        private static bool ContainsNode(List<StarMapNodeModel> nodes, ContentId nodeId)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].NodeId == nodeId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
