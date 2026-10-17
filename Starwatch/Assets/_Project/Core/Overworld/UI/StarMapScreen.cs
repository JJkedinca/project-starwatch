using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarMapModel"/> to the map surface and the selected-node card.
    /// </summary>
    internal sealed class StarMapScreen : IStarMapView
    {
        public event Action<ContentId> NodeSelected;

        public event Action TravelRequested;

        private readonly StarMapView _map = new();
        private readonly StarSystemInspectionView _inspection = new();

        internal StarMapView MapView => _map;

        internal void Bind(VisualElement mapSurface, VisualElement inspection)
        {
            _map.Bind(mapSurface);
            _map.NodeClicked += ClickNode;
            _inspection.Bind(inspection);
            _inspection.TravelClicked += ClickTravel;
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

            if (!model.HasSelection)
            {
                _inspection.ShowEmpty();
                return;
            }

            _inspection.ShowNode(model.Title, model.Summary, model.CanTravel);
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
            if (!card.HasSelection)
            {
                _inspection.ShowEmpty();
                return;
            }

            _inspection.ShowNode(card.Title, card.Summary, card.CanTravel);
        }

        internal void ClickNode(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void ClickTravel()
        {
            TravelRequested?.Invoke();
        }
    }
}
