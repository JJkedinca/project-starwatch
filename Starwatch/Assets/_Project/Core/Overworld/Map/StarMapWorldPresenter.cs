using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Presentation;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using VContainer;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// World-map presenter: pools and places 3D node, route, and fleet-marker views from models.
    /// </summary>
    public sealed class StarMapWorldPresenter : MonoBehaviour
    {
        private const float kParkedOffset = 1.2f;

        [SerializeField]
        private Transform _nodesRoot;

        [SerializeField]
        private Transform _routesRoot;

        [SerializeField]
        private Transform _fleetsRoot;

        [SerializeField]
        private MapNodeView _nodePrefab;

        [SerializeField]
        private MapRouteView _routePrefab;

        [SerializeField]
        private FleetMarkerView _fleetPrefab;

        private StarwatchPresentationDatabaseSO _presentationDatabase;
        private SimulationClock _clock;
        private readonly Dictionary<ContentId, MapNodeView> _nodes = new();
        private readonly Dictionary<ContentId, Vector3> _nodeWorld = new();
        private readonly List<MapRouteView> _routes = new();
        private readonly Dictionary<string, TransitMarker> _fleetMarkers = new();
        private readonly Stack<MapNodeView> _nodePool = new();
        private readonly Stack<MapRouteView> _routePool = new();
        private readonly Stack<FleetMarkerView> _fleetPool = new();
        private bool _hasCentered;
        private bool _listeningToClock;
        private bool _loggedMissingPresentation;

        internal int ActiveNodeCount
        {
            get
            {
                var count = 0;
                foreach (var pair in _nodes)
                {
                    if (pair.Value != null && pair.Value.isActiveAndEnabled && !pair.Value.NodeId.IsEmpty)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        internal event Action NodesRebuilt;

        [Inject]
        internal void Construct(
            SimulationClock clock,
            StarwatchPresentationDatabaseSO presentationDatabase)
        {
            _clock = clock;
            _presentationDatabase = presentationDatabase;
        }

        private void OnDisable()
        {
            UnsubscribeClock();
        }

        private void OnDestroy()
        {
            UnsubscribeClock();
        }

        internal void Clear()
        {
            foreach (var pair in _nodes)
            {
                ReleaseNode(pair.Value);
            }

            _nodes.Clear();
            _nodeWorld.Clear();
            ClearRoutes();
            ClearFleets();
            _hasCentered = false;
            NodesRebuilt?.Invoke();
        }

        internal void RebuildNodes(IReadOnlyList<StarMapNodeModel> nodes)
        {
            HashSet<ContentId> keep = new();
            var count = nodes?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var model = nodes[i];
                if (model.NodeId.IsEmpty)
                {
                    continue;
                }

                keep.Add(model.NodeId);
                var world = MapSpace.ToWorld(model.MapPosition);
                if (!_nodes.TryGetValue(model.NodeId, out var visual))
                {
                    visual = RentNode();
                    visual.name = "node-" + model.NodeId.Value;
                    _nodes[model.NodeId] = visual;
                }

                visual.Bind(model.NodeId);
                visual.transform.position = world;
                ApplyNodeVisual(visual, model.State);
                _nodeWorld[model.NodeId] = world;
            }

            List<ContentId> remove = null;
            foreach (var pair in _nodes)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<ContentId>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    var id = remove[i];
                    ReleaseNode(_nodes[id]);
                    _nodes.Remove(id);
                    _nodeWorld.Remove(id);
                }
            }

            NodesRebuilt?.Invoke();
        }

        internal Vector3? FocusWorldFromModels(IReadOnlyList<StarMapNodeModel> nodes)
        {
            if (nodes == null)
            {
                return null;
            }

            Vector3? firstVisible = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var model = nodes[i];
                if (model.NodeId.IsEmpty)
                {
                    continue;
                }

                var world = MapSpace.ToWorld(model.MapPosition);
                if (model.State.Visibility == MapNodeVisibility.Present)
                {
                    return world;
                }

                if (firstVisible == null
                    && (model.State.Visibility != MapNodeVisibility.Hidden || model.State.IsExplorable))
                {
                    firstVisible = world;
                }
            }

            return firstVisible;
        }

        internal MapCameraBounds BuildBounds()
        {
            List<Vector3> points = new(_nodeWorld.Count);
            foreach (var pair in _nodeWorld)
            {
                points.Add(pair.Value);
            }

            return MapCameraBounds.FromWorldPoints(points, MapCameraBounds.kDefaultMargin);
        }

        internal void SetNodeState(ContentId nodeId, MapNodePresentationState state)
        {
            if (!_nodes.TryGetValue(nodeId, out var visual))
            {
                return;
            }

            ApplyNodeVisual(visual, state);
        }

        internal void SetRoutes(IReadOnlyList<MapRouteSegment> routes)
        {
            ClearRoutes();
            var count = routes?.Count ?? 0;
            var presentation = TryGetStarMapPresentation();
            for (var i = 0; i < count; i++)
            {
                var visual = RentRoute();
                var segment = routes[i];
                if (presentation != null)
                {
                    visual.Apply(
                        segment,
                        presentation.RouteColor(segment.State),
                        presentation.RouteWidth(segment.State));
                }
                else
                {
                    visual.Apply(segment, Color.white, 0.12f);
                }

                _routes.Add(visual);
            }
        }

        internal void SetFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            HashSet<string> keep = new();
            var count = markers?.Count ?? 0;
            var hasTransit = false;
            var presentation = TryGetStarMapPresentation();
            for (var i = 0; i < count; i++)
            {
                var model = markers[i];
                var key = model.Key;
                keep.Add(key);
                if (!_fleetMarkers.TryGetValue(key, out var entry))
                {
                    entry = new TransitMarker(RentFleet());
                    _fleetMarkers[key] = entry;
                }

                var color = presentation != null
                    ? presentation.ResolveFleetColor(model.IsSelected)
                    : Color.white;
                entry.ApplyModel(model, color);
                hasTransit |= model.IsTransit;
            }

            List<string> remove = null;
            foreach (var pair in _fleetMarkers)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<string>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    var key = remove[i];
                    ReleaseFleet(_fleetMarkers[key].View);
                    _fleetMarkers.Remove(key);
                }
            }

            if (hasTransit)
            {
                SubscribeClock();
            }
            else
            {
                UnsubscribeClock();
            }
        }

        internal bool ConsumeNeedsCenter()
        {
            if (_hasCentered)
            {
                return false;
            }

            _hasCentered = true;
            return true;
        }

        internal bool TryGetNodeWorld(ContentId nodeId, out Vector3 worldPosition)
        {
            return _nodeWorld.TryGetValue(nodeId, out worldPosition);
        }

        private void ApplyNodeVisual(MapNodeView visual, MapNodePresentationState state)
        {
            if (visual == null)
            {
                return;
            }

            var presentation = TryGetStarMapPresentation();
            var color = presentation != null
                ? presentation.ResolveNodeColor(state)
                : Color.white;
            visual.ApplyState(state, color);
        }

        private StarMapPresentationSO TryGetStarMapPresentation()
        {
            if (_presentationDatabase != null && _presentationDatabase.StarMap != null)
            {
                return _presentationDatabase.StarMap;
            }

            if (!_loggedMissingPresentation)
            {
                _loggedMissingPresentation = true;
                Debug.LogWarning(
                    "StarMapWorldPresenter missing StarwatchPresentationDatabaseSO.StarMap; using fallback tints.",
                    this);
            }

            return null;
        }

        private void OnTimeAdvanced(GameTime time, float deltaSeconds)
        {
            if (_fleetMarkers.Count == 0)
            {
                return;
            }

            var anyTransit = false;
            foreach (var pair in _fleetMarkers)
            {
                if (pair.Value.Advance(deltaSeconds))
                {
                    anyTransit = true;
                }
            }

            if (!anyTransit)
            {
                UnsubscribeClock();
            }
        }

        private void SubscribeClock()
        {
            if (_listeningToClock || _clock == null)
            {
                return;
            }

            _clock.TimeAdvanced += OnTimeAdvanced;
            _listeningToClock = true;
        }

        private void UnsubscribeClock()
        {
            if (!_listeningToClock || _clock == null)
            {
                return;
            }

            _clock.TimeAdvanced -= OnTimeAdvanced;
            _listeningToClock = false;
        }

        private void ClearRoutes()
        {
            for (var i = 0; i < _routes.Count; i++)
            {
                ReleaseRoute(_routes[i]);
            }

            _routes.Clear();
        }

        private void ClearFleets()
        {
            foreach (var pair in _fleetMarkers)
            {
                ReleaseFleet(pair.Value.View);
            }

            _fleetMarkers.Clear();
            UnsubscribeClock();
        }

        private MapNodeView RentNode()
        {
            if (_nodePrefab == null)
            {
                Debug.LogError("StarMapWorldPresenter requires a node prefab.");
                return null;
            }

            var visual = _nodePool.Count > 0 ? _nodePool.Pop() : Instantiate(_nodePrefab, _nodesRoot);
            visual.gameObject.SetActive(true);
            return visual;
        }

        private void ReleaseNode(MapNodeView visual)
        {
            if (visual == null)
            {
                return;
            }

            visual.Bind(default);
            visual.gameObject.SetActive(false);
            _nodePool.Push(visual);
        }

        private MapRouteView RentRoute()
        {
            if (_routePrefab == null)
            {
                Debug.LogError("StarMapWorldPresenter requires a route prefab.");
                return null;
            }

            var visual = _routePool.Count > 0 ? _routePool.Pop() : Instantiate(_routePrefab, _routesRoot);
            visual.gameObject.SetActive(true);
            return visual;
        }

        private void ReleaseRoute(MapRouteView visual)
        {
            if (visual == null)
            {
                return;
            }

            visual.gameObject.SetActive(false);
            _routePool.Push(visual);
        }

        private FleetMarkerView RentFleet()
        {
            if (_fleetPrefab == null)
            {
                Debug.LogError("StarMapWorldPresenter requires a fleet marker prefab.");
                return null;
            }

            var visual = _fleetPool.Count > 0 ? _fleetPool.Pop() : Instantiate(_fleetPrefab, _fleetsRoot);
            visual.gameObject.SetActive(true);
            return visual;
        }

        private void ReleaseFleet(FleetMarkerView visual)
        {
            if (visual == null)
            {
                return;
            }

            visual.gameObject.SetActive(false);
            _fleetPool.Push(visual);
        }

        private sealed class TransitMarker
        {
            internal FleetMarkerView View { get; }

            private Vector3 _from;
            private Vector3 _to;
            private float _remaining;
            private float _total;
            private bool _isTransit;

            internal TransitMarker(FleetMarkerView view)
            {
                View = view;
            }

            internal void ApplyModel(FleetMarkerModel model, Color color)
            {
                _isTransit = model.IsTransit;
                _from = MapSpace.ToWorld(model.MapPosition);
                _to = MapSpace.ToWorld(model.DestinationMapPosition);
                _remaining = model.RemainingSeconds;
                _total = model.TotalSeconds;
                View.Apply(color, model.Count);
                RefreshPose();
            }

            internal bool Advance(float deltaSeconds)
            {
                if (!_isTransit)
                {
                    return false;
                }

                if (deltaSeconds > 0f)
                {
                    _remaining = Mathf.Max(0f, _remaining - deltaSeconds);
                }

                RefreshPose();
                return _remaining > 0.0001f;
            }

            private void RefreshPose()
            {
                if (!_isTransit)
                {
                    var parked = _from + new Vector3(0f, 0f, kParkedOffset);
                    View.SetPose(parked, 0f);
                    return;
                }

                var position = FleetTransitPlacement.Evaluate(_from, _to, _remaining, _total);
                var yaw = FleetTransitPlacement.YawDegrees(_from, _to);
                View.SetPose(position, yaw);
            }
        }
    }
}
