using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarMapModel"/> to the 3D world view, label overlay, and card.
    /// </summary>
    internal sealed class StarMapView : IStarMapView
    {
        public event Action<ContentId> NodeSelected;

        public event Action<ContentId> TravelRequested;

        public event Action<ContentId> NodeHovered;

        private readonly StarSystemInspectionView _inspection = new();
        private readonly MapLabelOverlay _labels = new();
        private StarMapWorldView _worldView;
        private MapCameraController _cameraController;
        private MapPointerInput _pointerInput;
        private OverworldSpaceSwitcher _spaceSwitcher;
        private ContentId _cardNodeId;
        private bool _wired;

        internal void Bind(
            VisualElement mapSurface,
            VisualElement inspection,
            StarMapWorldView worldView,
            MapCameraController cameraController,
            MapPointerInput pointerInput,
            OverworldSpaceSwitcher spaceSwitcher,
            Camera mapCamera)
        {
            _worldView = worldView;
            _cameraController = cameraController;
            _pointerInput = pointerInput;
            _spaceSwitcher = spaceSwitcher;
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

            if (_worldView != null)
            {
                _worldView.NodesRebuilt += OnNodesRebuilt;
            }
        }

        public void Apply(StarMapModel model)
        {
            if (model == null || !model.HasRun)
            {
                _worldView?.Clear();
                _labels.Clear();
                _inspection.ShowEmpty();
                _cardNodeId = default;
                return;
            }

            _worldView?.RebuildNodes(model.Nodes);
            ApplyCameraAfterRebuild(model.Nodes);
            _worldView?.SetRoutes(model.Routes);
            for (var i = 0; i < model.Nodes.Count; i++)
            {
                var row = model.Nodes[i];
                _worldView?.SetNodeState(row.NodeId, row.State);
            }

            if (IsGalaxyActive)
            {
                _labels.Rebuild(model.Nodes, _worldView);
            }
            else
            {
                _labels.Clear();
            }

            _worldView?.SetFleetMarkers(model.FleetMarkers);
            _cardNodeId = model.Card.NodeId;
            _inspection.Show(model.Card);
        }

        public void ApplyNodes(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (nodes == null || _worldView == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var row = nodes[i];
                _worldView.SetNodeState(row.NodeId, row.State);
                if (IsGalaxyActive)
                {
                    _labels.SetNodeState(row.NodeId, row.State, _worldView);
                }
            }
        }

        public void ApplyRoutes(IReadOnlyList<MapRouteSegment> routes)
        {
            _worldView?.SetRoutes(routes);
        }

        public void ApplyCard(StarMapCardModel card)
        {
            _cardNodeId = card.NodeId;
            _inspection.Show(card);
        }

        public void ApplyHoverTooltip(StarMapHoverTooltipModel tooltip)
        {
            if (!IsGalaxyActive)
            {
                return;
            }

            _labels.SetHoverTooltip(tooltip);
        }

        public void ApplyFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            _worldView?.SetFleetMarkers(markers);
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
            if (!nodeId.IsEmpty && _worldView != null && _worldView.TryGetNodeWorld(nodeId, out var world))
            {
                _labels.RememberWorld(nodeId, world);
            }

            NodeHovered?.Invoke(nodeId);
        }

        private void OnCameraMoved()
        {
            if (!IsGalaxyActive)
            {
                return;
            }

            _labels.RepositionAll();
        }

        private void OnNodesRebuilt()
        {
            if (!IsGalaxyActive)
            {
                return;
            }

            _labels.RepositionAll();
        }

        private bool IsGalaxyActive => _spaceSwitcher == null || _spaceSwitcher.IsGalaxyActive;

        private void ApplyCameraAfterRebuild(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (_worldView == null || !IsGalaxyActive)
            {
                return;
            }

            var bounds = _worldView.BuildBounds();
            _cameraController?.SetBounds(bounds);
            if (_cameraController == null || !_worldView.ConsumeNeedsCenter())
            {
                return;
            }

            var focus = _worldView.FocusWorldFromModels(nodes);
            if (focus.HasValue)
            {
                _cameraController.CenterOn(focus.Value, instant: true);
            }
        }
    }
}
