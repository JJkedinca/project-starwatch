using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarMapModel"/> to the map surface and the selected-node card.
    /// </summary>
    internal sealed class StarMapScreen : IStarMapView
    {
        public event Action<ContentId> NodeSelected;

        public event Action<ContentId> TravelRequested;

        public event Action<ContentId> NodeHovered;

        private readonly StarMapView _map = new();
        private readonly StarSystemInspectionView _inspection = new();

        internal StarMapView MapView => _map;

        internal void Bind(VisualElement mapSurface, VisualElement inspection)
        {
            _map.Bind(mapSurface);
            _map.NodeClicked += ClickNode;
            _map.NodeRightClicked += ClickTravel;
            _map.NodeHovered += id => NodeHovered?.Invoke(id);
            _inspection.Bind(inspection);
        }

        public void Apply(StarMapModel model)
        {
            if (model == null || !model.HasRun)
            {
                _map.Clear();
                _inspection.ShowEmpty();
                return;
            }

            _map.Rebuild(model.Nodes);
            _map.SetRoutes(model.Routes);
            for (var i = 0; i < model.Nodes.Count; i++)
            {
                var row = model.Nodes[i];
                _map.SetNodeState(row.NodeId, row.State);
            }

            _map.SetFleetMarkers(model.FleetMarkers);
            _inspection.Show(model.Card);
        }

        public void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var row = nodes[i];
                _map.SetNodeState(row.NodeId, row.State);
            }
        }

        public void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes)
        {
            _map.SetRoutes(routes);
        }

        public void ApplyCard(StarMapCardModel card)
        {
            _inspection.Show(card);
        }

        public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
        {
            _map.SetHoverTooltip(tooltip);
        }

        public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            _map.SetFleetMarkers(markers);
        }

        internal void ClickNode(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void ClickTravel(ContentId destinationNodeId)
        {
            TravelRequested?.Invoke(destinationNodeId);
        }
    }
}
