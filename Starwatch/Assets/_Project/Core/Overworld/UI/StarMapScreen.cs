using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarMapModel"/> to the 3D world presenter, label overlay, and card.
    /// </summary>
    internal sealed class StarMapScreen : IStarMapView
    {
        public event Action<ContentId> NodeSelected;

        public event Action<ContentId> TravelRequested;

        public event Action<ContentId> NodeHovered;

        private readonly StarSystemInspectionView _inspection = new();
        private readonly MapLabelOverlay _labels = new();
        private StarMapWorldPresenter _worldPresenter;
        private MapCameraController _cameraController;
        private MapPointerInput _pointerInput;
        private ContentId _cardNodeId;
        private bool _wired;

        internal void Bind(
            VisualElement mapSurface,
            VisualElement inspection,
            StarMapWorldPresenter worldPresenter,
            MapCameraController cameraController,
            MapPointerInput pointerInput,
            Camera mapCamera)
        {
            _worldPresenter = worldPresenter;
            _cameraController = cameraController;
            _pointerInput = pointerInput;
            _inspection.Bind(inspection);
            _inspection.TravelClicked += OnInspectionTravel;
            _labels.Bind(mapSurface, mapCamera);

            if (_wired)
            {
                return;
            }

            _wired = true;
            if (_pointerInput != null)
            {
                _pointerInput.NodeClicked += ClickNode;
                _pointerInput.NodeRightClicked += ClickTravel;
                _pointerInput.NodeHovered += OnPointerHover;
            }

            if (_cameraController != null)
            {
                _cameraController.Moved += OnCameraMoved;
            }

            if (_worldPresenter != null)
            {
                _worldPresenter.NodesRebuilt += OnNodesRebuilt;
            }
        }

        public void Apply(StarMapModel model)
        {
            if (model == null || !model.HasRun)
            {
                _worldPresenter?.Clear();
                _labels.Clear();
                _inspection.ShowEmpty();
                _cardNodeId = default;
                return;
            }

            _worldPresenter?.RebuildNodes(model.Nodes);
            ApplyCameraAfterRebuild(model.Nodes);
            _worldPresenter?.SetRoutes(model.Routes);
            for (var i = 0; i < model.Nodes.Count; i++)
            {
                var row = model.Nodes[i];
                _worldPresenter?.SetNodeState(row.NodeId, row.State);
            }

            _labels.Rebuild(model.Nodes, _worldPresenter);
            _worldPresenter?.SetFleetMarkers(model.FleetMarkers);
            _cardNodeId = model.Card.NodeId;
            _inspection.Show(model.Card);
        }

        public void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (nodes == null || _worldPresenter == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var row = nodes[i];
                _worldPresenter.SetNodeState(row.NodeId, row.State);
                _labels.SetNodeState(row.NodeId, row.State, _worldPresenter);
            }
        }

        public void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes)
        {
            _worldPresenter?.SetRoutes(routes);
        }

        public void ApplyCard(StarMapCardModel card)
        {
            _cardNodeId = card.NodeId;
            _inspection.Show(card);
        }

        public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
        {
            _labels.SetHoverTooltip(tooltip);
        }

        public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            _worldPresenter?.SetFleetMarkers(markers);
        }

        internal void ClickNode(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void ClickTravel(ContentId destinationNodeId)
        {
            TravelRequested?.Invoke(destinationNodeId);
        }

        private void OnInspectionTravel()
        {
            if (_cardNodeId.IsEmpty)
            {
                return;
            }

            TravelRequested?.Invoke(_cardNodeId);
        }

        private void OnPointerHover(ContentId nodeId)
        {
            _labels.SetHover(nodeId);
            if (!nodeId.IsEmpty && _worldPresenter != null && _worldPresenter.TryGetNodeWorld(nodeId, out var world))
            {
                _labels.RememberWorld(nodeId, world);
            }

            NodeHovered?.Invoke(nodeId);
        }

        private void OnCameraMoved()
        {
            _labels.RepositionAll();
        }

        private void OnNodesRebuilt()
        {
            _labels.RepositionAll();
        }

        private void ApplyCameraAfterRebuild(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (_worldPresenter == null)
            {
                return;
            }

            var bounds = _worldPresenter.BuildBounds();
            _cameraController?.SetBounds(bounds);
            if (_cameraController == null || !_worldPresenter.ConsumeNeedsCenter())
            {
                return;
            }

            var focus = _worldPresenter.FocusWorldFromModels(nodes);
            if (focus.HasValue)
            {
                _cameraController.CenterOn(focus.Value, instant: true);
            }
        }
    }
}
