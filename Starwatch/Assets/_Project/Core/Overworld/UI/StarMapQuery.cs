using System;
using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld.Services;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Read model for the star map and the selected-node card.
    /// </summary>
    internal sealed class StarMapQuery
    {
        private readonly VisibilityService _visibility;
        private readonly TravelService _travel;

        public StarMapQuery(VisibilityService visibility, TravelService travel)
        {
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
        }

        /// <summary>
        /// Nodes, known routes, and the card for the selected node.
        /// </summary>
        internal StarMapModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null)
            {
                return StarMapModel.Empty;
            }

            var selectedNodeId = selection == null ? default : selection.SelectedNodeId;
            var inStarSystem = selection != null && selection.InStarSystem;
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

            var card = DescribeCard(run, selectedNodeId, inStarSystem);
            return new StarMapModel(
                true,
                nodes,
                KnownRoutes(run),
                card.HasSelection,
                card.Title,
                card.Summary,
                card.CanTravel);
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

        internal MapNodePresentationState NodeState(RunState run, MapNode node, ContentId selectedNodeId)
        {
            var visibility = _visibility.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, node.Id);
            var hasFleet = visibility != MapNodeVisibility.Hidden && run.Timeline.HasFleetAt(node.Id);
            return new MapNodePresentationState(
                visibility,
                isExplorable: visibility == MapNodeVisibility.Hidden && IsKnownNeighbor(run, node.Id),
                isSelected: node.Id == selectedNodeId,
                hasFleet,
                isFleetTraveling: hasFleet && run.Timeline.HasTravelingFleetAt(node.Id),
                revealedLabel: visibility == MapNodeVisibility.Hidden ? string.Empty : NodeLabel(node));
        }

        internal StarMapCardModel DescribeCard(RunState run, OverworldSelection selection)
        {
            if (run == null || selection == null)
            {
                return StarMapCardModel.Empty;
            }

            return DescribeCard(run, selection.SelectedNodeId, selection.InStarSystem);
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

        private StarMapCardModel DescribeCard(RunState run, ContentId selectedNodeId, bool inStarSystem)
        {
            if (selectedNodeId.IsEmpty || !run.Universe.MapGraph.TryGetNode(selectedNodeId, out var node))
            {
                return StarMapCardModel.Empty;
            }

            var visibility = _visibility.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, node.Id);
            var canTravel = TryFindFleetForDestination(run, node.Id, out _);
            if (visibility == MapNodeVisibility.Hidden)
            {
                return new StarMapCardModel(
                    true,
                    "Uncharted space",
                    canTravel
                        ? "Known route. Travel here to enter."
                        : "Hidden until a fleet arrives by a known route.",
                    canTravel);
            }

            var kindLabel = node.Kind == MapNodeKind.System ? "Star system" : "Non-system node";
            var summary = kindLabel + " · " + node.Id.Value;
            if (visibility == MapNodeVisibility.Present)
            {
                summary += "\nFleet present.";
            }

            if (!inStarSystem)
            {
                summary += node.PlanetIds.Count > 0
                    ? "\nOpen to inspect planets."
                    : "\nOpen to inspect.";
            }

            return new StarMapCardModel(true, NodeLabel(node), summary, canTravel);
        }

        private bool IsKnownNeighbor(RunState run, ContentId nodeId)
        {
            var neighbors = run.Universe.MapGraph.GetNeighbors(nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                var neighborId = neighbors[i];
                if (run.Timeline.HasFleetAt(neighborId)
                    && _travel.IsKnownAdjacentDestination(run, ObserverId.Player, neighborId, nodeId))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NodeLabel(MapNode node)
        {
            return node == null || node.Id.IsEmpty ? string.Empty : node.Id.Value;
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
