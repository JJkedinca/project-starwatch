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

        public event Action<EntityId> FleetSelected;

        public event Action<ContentId> TravelRequested;

        private readonly StarMapView _map = new();
        private readonly StarSystemInspectionView _inspection = new();
        private readonly FleetListView _fleets = new();

        internal StarMapView MapView => _map;

        internal void Bind(VisualElement mapSurface, VisualElement inspection)
        {
            _map.Bind(mapSurface);
            _map.NodeClicked += ClickNode;
            _map.NodeRightClicked += ClickTravel;
            _inspection.Bind(inspection);
            _fleets.Bind(inspection != null ? inspection.Q<VisualElement>("fleet-list") : null);
            _fleets.FleetSelected += ClickFleet;
        }

        public void Apply(StarMapModel model)
        {
            if (model == null || !model.HasRun)
            {
                _map.Clear();
                _inspection.ShowEmpty();
                _fleets.Apply(Array.Empty<FleetChoiceModel>());
                return;
            }

            _map.Rebuild(model.Nodes);
            _map.SetRoutes(model.Routes);
            _fleets.Apply(model.Fleets);
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

            _inspection.ShowNode(model.Title, model.Summary, model.HasArrival, model.ArrivalSeconds);
        }

        public void ApplyFleets(IReadOnlyList<FleetChoiceModel> fleets)
        {
            _fleets.Apply(fleets);
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

            _inspection.ShowNode(card.Title, card.Summary, card.HasArrival, card.ArrivalSeconds);
        }

        internal void ClickNode(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void ClickFleet(EntityId fleetId)
        {
            FleetSelected?.Invoke(fleetId);
        }

        internal void ClickTravel(ContentId destinationNodeId)
        {
            TravelRequested?.Invoke(destinationNodeId);
        }
    }
}
