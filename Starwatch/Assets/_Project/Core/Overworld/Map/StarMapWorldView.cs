using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Presentation;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Galaxy-map world view: pools and places 3D node, route, and fleet-marker views from models.
    /// </summary>
    public sealed class StarMapWorldView : MonoBehaviour
    {
        private const float kParkedOffset = 1.2f;

        [SerializeField]
        private Transform _nodesRoot;

        [SerializeField]
        private Transform _routesRoot;

        [SerializeField]
        private Transform _fleetsRoot;

        // TODO: Nodes will be a SO containing different kinds per system type
        [SerializeField]
        private MapNodeView _nodePrefab;

        [SerializeField]
        private MapRouteView _routePrefab;

        [SerializeField]
        private FleetMarkerView _fleetPrefab;

        private StarMapPresentationSO _presentation;
        private SimulationClock _clock;
        private readonly Dictionary<ContentId, MapNodeView> _nodes = new();
        private readonly Dictionary<ContentId, Vector3> _nodeWorld = new();
        private readonly List<MapRouteView> _routes = new();
        private readonly Dictionary<string, TransitMarker> _fleetMarkers = new();
        private ObjectPool<MapNodeView> _nodePool;
        private ObjectPool<MapRouteView> _routePool;
        private ObjectPool<FleetMarkerView> _fleetPool;
        private bool _hasCentered;
        private bool _listeningToClock;

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
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (presentationDatabase == null)
            {
                throw new ArgumentNullException(nameof(presentationDatabase));
            }

            _presentation = presentationDatabase.StarMap
                ?? throw new ArgumentException(
                    "StarwatchPresentationDatabaseSO.StarMap is required.",
                    nameof(presentationDatabase));
        }

        private void Awake()
        {
            _nodePool = new ObjectPool<MapNodeView>(
                createFunc: () => Instantiate(_nodePrefab, _nodesRoot),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: visual =>
                {
                    visual.Bind(default);
                    visual.gameObject.SetActive(false);
                });
            _routePool = new ObjectPool<MapRouteView>(
                createFunc: () => Instantiate(_routePrefab, _routesRoot),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: visual => visual.gameObject.SetActive(false));
            _fleetPool = new ObjectPool<FleetMarkerView>(
                createFunc: () => Instantiate(_fleetPrefab, _fleetsRoot),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: visual => visual.gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            UnsubscribeClock();
        }

        private void OnDestroy()
        {
            UnsubscribeClock();
            _nodePool?.Dispose();
            _routePool?.Dispose();
            _fleetPool?.Dispose();
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

            PooledMapSync.ReleaseAbsent(_nodes, keep, (id, visual) =>
            {
                ReleaseNode(visual);
                _nodeWorld.Remove(id);
            });

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
            for (var i = 0; i < count; i++)
            {
                var visual = RentRoute();
                var segment = routes[i];
                visual.Apply(
                    segment,
                    _presentation.RouteColor(segment.State),
                    _presentation.RouteWidth(segment.State));
                _routes.Add(visual);
            }
        }

        internal void SetFleetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            HashSet<string> keep = new();
            var count = markers?.Count ?? 0;
            var hasTransit = false;
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

                entry.ApplyModel(model, _presentation.ResolveFleetColor(model.IsSelected));
                hasTransit |= model.IsTransit;
            }

            PooledMapSync.ReleaseAbsent(_fleetMarkers, keep, (_, entry) => ReleaseFleet(entry.View));

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

            visual.ApplyState(state, _presentation.ResolveNodeColor(state));
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
                Debug.LogError("StarMapWorldView requires a node prefab.");
                return null;
            }

            return _nodePool.Get();
        }

        private void ReleaseNode(MapNodeView visual)
        {
            if (visual == null)
            {
                return;
            }

            _nodePool.Release(visual);
        }

        private MapRouteView RentRoute()
        {
            if (_routePrefab == null)
            {
                Debug.LogError("StarMapWorldView requires a route prefab.");
                return null;
            }

            return _routePool.Get();
        }

        private void ReleaseRoute(MapRouteView visual)
        {
            if (visual == null)
            {
                return;
            }

            _routePool.Release(visual);
        }

        private FleetMarkerView RentFleet()
        {
            if (_fleetPrefab == null)
            {
                Debug.LogError("StarMapWorldView requires a fleet marker prefab.");
                return null;
            }

            return _fleetPool.Get();
        }

        private void ReleaseFleet(FleetMarkerView visual)
        {
            if (visual == null)
            {
                return;
            }

            _fleetPool.Release(visual);
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
