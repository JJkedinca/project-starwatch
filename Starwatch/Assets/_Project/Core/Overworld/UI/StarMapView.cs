using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Owns the map surface, route layer, hover tooltip, and map-node element lifecycle.
    /// </summary>
    internal sealed class StarMapView
    {
        public event Action<ContentId> NodeClicked;

        public event Action<ContentId> NodeRightClicked;

        /// <summary>
        /// Raised when the pointer enters a node, or with <see cref="ContentId"/> empty on leave.
        /// </summary>
        public event Action<ContentId> NodeHovered;

        private VisualElement _mapSurface;
        private RouteView _routes;
        private FleetMarkerView _fleetMarkers;
        private Label _hoverTooltip;
        private readonly Dictionary<ContentId, MapNodeView> _nodesById = new();
        private readonly List<MapRouteSegment> _routeSegments = new();
        private float _scale = MapLayout.kDefaultScale;
        private Vector2 _originOffset = MapLayout.kDefaultPadding;
        private ContentId _hoveredNodeId;

        /// <summary>
        /// Binds to the map-surface element from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement mapSurface)
        {
            _mapSurface = mapSurface;
            EnsureRouteLayer();
            EnsureFleetMarkerLayer();
            EnsureHoverTooltip();
        }

        /// <summary>
        /// Creates node elements when the set of nodes changes. An existing node stays in
        /// place so a click can refresh without removing that button.
        /// </summary>
        public void Rebuild(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (_mapSurface == null)
            {
                return;
            }

            List<StarMapNodeModel> validNodes = new();
            List<Vector2> positions = new();
            var count = nodes?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var node = nodes[i];
                if (node.NodeId.IsEmpty)
                {
                    continue;
                }

                validNodes.Add(node);
                positions.Add(node.MapPosition);
            }

            if (SameNodes(validNodes))
            {
                return;
            }

            List<MapRouteSegment> preservedRoutes = null;
            if (_routeSegments.Count > 0)
            {
                preservedRoutes = new List<MapRouteSegment>(_routeSegments);
            }

            ClearNodes();
            EnsureRouteLayer();
            EnsureFleetMarkerLayer();
            EnsureHoverTooltip();
            _originOffset = MapLayout.ComputeOriginOffset(positions, _scale, MapLayout.kDefaultPadding);
            _routes.SetTransform(_scale, _originOffset);
            _fleetMarkers.SetTransform(_scale, _originOffset);

            for (var i = 0; i < validNodes.Count; i++)
            {
                var node = validNodes[i];
                var view = new MapNodeView(node.NodeId, node.Kind);
                view.Clicked += id => NodeClicked?.Invoke(id);
                view.RightClicked += id => NodeRightClicked?.Invoke(id);
                view.PointerEntered += OnNodePointerEntered;
                view.PointerLeft += OnNodePointerLeft;
                view.SetMapPosition(node.MapPosition, _scale, _originOffset);
                _nodesById[node.NodeId] = view;
                _mapSurface.Add(view.Root);
            }

            BringTooltipToFront();

            if (preservedRoutes != null)
            {
                _routeSegments.Clear();
                for (var i = 0; i < preservedRoutes.Count; i++)
                {
                    _routeSegments.Add(preservedRoutes[i]);
                }

                _routes.SetSegments(_routeSegments);
            }
        }

        /// <summary>
        /// Replaces the drawn route set when it changes.
        /// </summary>
        public void SetRoutes(IReadOnlyList<MapRouteSegment> segments)
        {
            if (SameRoutes(segments))
            {
                return;
            }

            _routeSegments.Clear();
            if (segments != null)
            {
                for (var i = 0; i < segments.Count; i++)
                {
                    _routeSegments.Add(segments[i]);
                }
            }

            EnsureRouteLayer();
            _routes.SetTransform(_scale, _originOffset);
            _routes.SetSegments(_routeSegments);
        }

        /// <summary>
        /// Replaces parked and in-transit fleet arrows. Transit markers start a USS move.
        /// </summary>
        public void SetFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            EnsureFleetMarkerLayer();
            _fleetMarkers.SetTransform(_scale, _originOffset);
            _fleetMarkers.SetMarkers(markers);
            BringMarkersAboveRoutes();
            BringTooltipToFront();
        }

        public void SetHoverTooltip(StarMapHoverTooltipModel tooltip)
        {
            EnsureHoverTooltip();
            if (_hoverTooltip == null)
            {
                return;
            }

            if (!tooltip.IsShown || _hoveredNodeId.IsEmpty || !_nodesById.TryGetValue(_hoveredNodeId, out var node))
            {
                _hoverTooltip.style.display = DisplayStyle.None;
                return;
            }

            _hoverTooltip.text = tooltip.Reachable
                ? tooltip.TargetName + " · " + tooltip.Seconds.ToString("0.#") + "s"
                : tooltip.TargetName + " · unreachable";
            var left = node.Root.style.left.value.value + MapLayout.kNodeSize + 8f;
            var top = node.Root.style.top.value.value;
            _hoverTooltip.style.left = left;
            _hoverTooltip.style.top = top;
            _hoverTooltip.style.display = DisplayStyle.Flex;
            node.Root.BringToFront();
            BringTooltipToFront();
        }

        /// <summary>
        /// Updates styling for one node.
        /// </summary>
        public void SetNodeState(ContentId nodeId, MapNodePresentationState state)
        {
            if (!_nodesById.TryGetValue(nodeId, out var view))
            {
                return;
            }

            view.ApplyState(state);
        }

        /// <summary>
        /// Removes all node and route elements from the map surface.
        /// </summary>
        public void Clear()
        {
            ClearNodes();
            _routes?.Clear();
            _routeSegments.Clear();
            _fleetMarkers?.Clear();
        }

        private void ClearNodes()
        {
            // Rebuild removes nodes under the pointer without a leave event; clear hover
            // so the presenter drops a stale dashed course preview.
            if (!_hoveredNodeId.IsEmpty)
            {
                _hoveredNodeId = default;
                NodeHovered?.Invoke(default);
            }

            if (_hoverTooltip != null)
            {
                _hoverTooltip.style.display = DisplayStyle.None;
            }

            foreach (var pair in _nodesById)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _nodesById.Clear();
        }

        /// <summary>
        /// Returns whether a node element exists on the map.
        /// </summary>
        public bool HasNode(ContentId nodeId)
        {
            return _nodesById.ContainsKey(nodeId);
        }

        /// <summary>
        /// Number of node elements currently on the map.
        /// </summary>
        public int NodeCount => _nodesById.Count;

        /// <summary>
        /// Number of route segments currently drawn.
        /// </summary>
        public int RouteCount => _routes != null ? _routes.SegmentCount : 0;

        /// <summary>
        /// Number of fleet marker elements currently on the map.
        /// </summary>
        public int FleetMarkerCount => _fleetMarkers != null ? _fleetMarkers.MarkerCount : 0;

        private void OnNodePointerEntered(ContentId nodeId)
        {
            _hoveredNodeId = nodeId;
            if (_nodesById.TryGetValue(nodeId, out var view))
            {
                view.Root.BringToFront();
                BringTooltipToFront();
            }

            NodeHovered?.Invoke(nodeId);
        }

        private void OnNodePointerLeft(ContentId nodeId)
        {
            if (_hoveredNodeId != nodeId)
            {
                return;
            }

            _hoveredNodeId = default;
            NodeHovered?.Invoke(default);
        }

        private bool SameNodes(List<StarMapNodeModel> nodes)
        {
            if (_nodesById.Count != nodes.Count)
            {
                return false;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                if (!_nodesById.ContainsKey(nodes[i].NodeId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool SameRoutes(IReadOnlyList<MapRouteSegment> segments)
        {
            var count = segments?.Count ?? 0;
            if (_routeSegments.Count != count)
            {
                return false;
            }

            var matched = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var segment = segments[i];
                var found = false;
                for (var j = 0; j < count; j++)
                {
                    if (matched[j] || !SameSegment(_routeSegments[j], segment))
                    {
                        continue;
                    }

                    matched[j] = true;
                    found = true;
                    break;
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool SameSegment(MapRouteSegment left, MapRouteSegment right)
        {
            return left.State == right.State
                && left.NodeAId == right.NodeAId
                && left.NodeBId == right.NodeBId
                && left.MapPositionA == right.MapPositionA
                && left.MapPositionB == right.MapPositionB;
        }

        private void EnsureRouteLayer()
        {
            if (_mapSurface == null)
            {
                return;
            }

            if (_routes == null)
            {
                _routes = new RouteView();
            }

            if (_routes.Root.parent == _mapSurface)
            {
                return;
            }

            _mapSurface.Insert(0, _routes.Root);
        }

        private void EnsureFleetMarkerLayer()
        {
            if (_mapSurface == null)
            {
                return;
            }

            if (_fleetMarkers == null)
            {
                _fleetMarkers = new FleetMarkerView();
            }

            if (_fleetMarkers.Root.parent == _mapSurface)
            {
                return;
            }

            // Above routes, below nodes and tooltip.
            var insertAt = _routes != null && _routes.Root.parent == _mapSurface ? 1 : 0;
            _mapSurface.Insert(insertAt, _fleetMarkers.Root);
        }

        private void EnsureHoverTooltip()
        {
            if (_mapSurface == null)
            {
                return;
            }

            if (_hoverTooltip != null && _hoverTooltip.parent == _mapSurface)
            {
                return;
            }

            _hoverTooltip = new Label();
            _hoverTooltip.name = "map-hover-tooltip";
            _hoverTooltip.AddToClassList("map-hover-tooltip");
            _hoverTooltip.pickingMode = PickingMode.Ignore;
            _hoverTooltip.style.display = DisplayStyle.None;
            _mapSurface.Add(_hoverTooltip);
        }

        private void BringMarkersAboveRoutes()
        {
            // Routes stay at the back; markers sit above nodes so a transit arrow stays visible.
            foreach (var pair in _nodesById)
            {
                pair.Value.Root.BringToFront();
            }

            if (_fleetMarkers != null && _fleetMarkers.Root.parent == _mapSurface)
            {
                _fleetMarkers.Root.BringToFront();
            }
        }

        private void BringTooltipToFront()
        {
            if (_hoverTooltip != null && _hoverTooltip.parent == _mapSurface)
            {
                _hoverTooltip.BringToFront();
            }
        }
    }
}
