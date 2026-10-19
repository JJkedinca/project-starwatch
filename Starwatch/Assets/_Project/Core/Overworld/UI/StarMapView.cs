using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Owns the map surface, route layer, and map-node element lifecycle.
    /// </summary>
    internal sealed class StarMapView
    {
        public event Action<ContentId> NodeClicked;

        public event Action<ContentId> NodeRightClicked;

        private VisualElement _mapSurface;
        private VisualElement _routeLayer;
        private readonly Dictionary<ContentId, MapNodeView> _nodesById = new();
        private readonly List<RouteView> _routes = new();
        private readonly List<MapRouteSegment> _routeSegments = new();
        private float _scale = MapLayout.kDefaultScale;
        private Vector2 _originOffset = MapLayout.kDefaultPadding;

        /// <summary>
        /// Binds to the map-surface element from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement mapSurface)
        {
            _mapSurface = mapSurface;
            EnsureRouteLayer();
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

            Clear();
            EnsureRouteLayer();
            _originOffset = MapLayout.ComputeOriginOffset(positions, _scale, MapLayout.kDefaultPadding);

            for (var i = 0; i < validNodes.Count; i++)
            {
                var node = validNodes[i];
                var view = new MapNodeView(node.NodeId, node.Kind);
                view.Clicked += id => NodeClicked?.Invoke(id);
                view.RightClicked += id => NodeRightClicked?.Invoke(id);
                view.SetMapPosition(node.MapPosition, _scale, _originOffset);
                _nodesById[node.NodeId] = view;
                _mapSurface.Add(view.Root);
            }
        }

        /// <summary>
        /// Creates route lines when the set of segments changes. An existing line stays in
        /// place so a map refresh can update nodes without redrawing the same routes.
        /// </summary>
        public void SetRoutes(IReadOnlyList<MapRouteSegment> segments)
        {
            if (SameRoutes(segments))
            {
                return;
            }

            ClearRoutes();
            EnsureRouteLayer();
            if (_routeLayer == null || segments == null)
            {
                return;
            }

            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var route = new RouteView();
                route.SetLocalEndpoints(
                    MapLayout.ToNodeCenter(segment.MapPositionA, _scale, _originOffset),
                    MapLayout.ToNodeCenter(segment.MapPositionB, _scale, _originOffset));
                _routes.Add(route);
                _routeSegments.Add(segment);
                _routeLayer.Add(route.Root);
            }
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
            ClearRoutes();
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
        /// Number of route elements currently on the map.
        /// </summary>
        public int RouteCount => _routes.Count;

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
            return left.MapPositionA == right.MapPositionA && left.MapPositionB == right.MapPositionB;
        }

        private void EnsureRouteLayer()
        {
            if (_mapSurface == null)
            {
                return;
            }

            if (_routeLayer != null && _routeLayer.parent == _mapSurface)
            {
                return;
            }

            _routeLayer = new VisualElement();
            _routeLayer.name = "route-layer";
            _routeLayer.AddToClassList("map-route-layer");
            _routeLayer.pickingMode = PickingMode.Ignore;
            _mapSurface.Insert(0, _routeLayer);
        }

        private void ClearRoutes()
        {
            for (var i = 0; i < _routes.Count; i++)
            {
                _routes[i].Root.RemoveFromHierarchy();
            }

            _routes.Clear();
            _routeSegments.Clear();
        }
    }
}
