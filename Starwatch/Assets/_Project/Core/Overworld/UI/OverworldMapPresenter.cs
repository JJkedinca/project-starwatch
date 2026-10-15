using System;
using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld.Services;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Overworld map presenter. Binds UI intents to travel and scan, then refreshes views
    /// from domain events published after those actions commit.
    /// </summary>
    internal sealed class OverworldMapPresenter : IStartable, IDisposable
    {
        private readonly OverworldViewBridge _viewBridge;
        private readonly RunManager _runManager;
        private readonly SensorService _sensor;
        private readonly TravelService _travel;
        private readonly OverworldMapQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        private ContentId _selectedNodeId;
        private ContentId _selectedPlanetId;
        private bool _inStarSystemView;

        internal OverworldMapPresenter(
            OverworldViewBridge viewBridge,
            RunManager runManager,
            SensorService sensor,
            TravelService travel,
            OverworldMapQuery query,
            DomainEventBus events)
        {
            _viewBridge = viewBridge ?? throw new ArgumentNullException(nameof(viewBridge));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        /// <summary>
        /// Binds UI events and builds the initial map from the active run.
        /// </summary>
        public void Start()
        {
            _viewBridge.NodeSelected += OnNodeSelected;
            _viewBridge.TravelRequested += OnTravelRequested;
            _viewBridge.ScanPlanetRequested += OnScanPlanetRequested;
            _viewBridge.StarSystemBackRequested += OnStarSystemBackRequested;
            _viewBridge.PlanetSelected += OnPlanetSelected;
            _viewBridge.Bind();
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(OnFleetTravel));
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            RefreshAll();
        }

        /// <summary>
        /// Drops event subscriptions when the overworld scope ends.
        /// </summary>
        public void Dispose()
        {
            _viewBridge.NodeSelected -= OnNodeSelected;
            _viewBridge.TravelRequested -= OnTravelRequested;
            _viewBridge.ScanPlanetRequested -= OnScanPlanetRequested;
            _viewBridge.StarSystemBackRequested -= OnStarSystemBackRequested;
            _viewBridge.PlanetSelected -= OnPlanetSelected;

            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        /// <summary>
        /// Rebuilds map nodes and inspection from the active run.
        /// </summary>
        private void RefreshAll()
        {
            if (!_runManager.HasActiveRun)
            {
                _inStarSystemView = false;
                _selectedPlanetId = default;
                _viewBridge.ShowStarMap();
                _viewBridge.StarMap.Clear();
                _viewBridge.StarSystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                _viewBridge.SetStatus("No active run.");
                return;
            }

            var run = _runManager.CurrentRun;
            _viewBridge.StarMap.Rebuild(run.Universe.MapGraph.Nodes);
            RefreshRoutes(run);
            RefreshNodeStates(run);

            if (_inStarSystemView)
            {
                EnterStarSystemView(run, keepSelectedPlanet: true);
                return;
            }

            _viewBridge.ShowStarMap();
            RefreshInspection(run);
            _viewBridge.SetStatus("Travel a known route to enter a star system, then scan its planets.");
        }

        private void OnNodeSelected(ContentId nodeId)
        {
            _selectedNodeId = nodeId;
            _selectedPlanetId = default;
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            RefreshNodeStates(run);

            if (_query.CanEnter(run, nodeId))
            {
                EnterStarSystemView(run, keepSelectedPlanet: false);
                return;
            }

            _inStarSystemView = false;
            _viewBridge.ShowStarMap();
            RefreshInspection(run);
        }

        private void OnStarSystemBackRequested()
        {
            _inStarSystemView = false;
            _selectedPlanetId = default;
            _viewBridge.ShowStarMap();
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            RefreshNodeStates(_runManager.CurrentRun);
            RefreshInspection(_runManager.CurrentRun);
            _viewBridge.SetStatus("Travel a known route to enter a star system, then scan its planets.");
        }

        private void OnPlanetSelected(ContentId planetId)
        {
            _selectedPlanetId = planetId;
            if (!_runManager.HasActiveRun || !_inStarSystemView)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            RefreshSystemPlanetStates(run);
            RefreshInspection(run);
        }

        private void OnTravelRequested()
        {
            if (!_runManager.HasActiveRun || _selectedNodeId.IsEmpty)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            if (!_query.TryFindFleetForDestination(run, _selectedNodeId, out var fleetId))
            {
                _viewBridge.SetStatus("Travel is not available.");
                return;
            }

            var result = _travel.StartTravel(run, ObserverId.Player, fleetId, _selectedNodeId);
            if (!result.Success)
            {
                _viewBridge.SetStatus(result.Error.Message);
                return;
            }
        }

        private void OnScanPlanetRequested(ContentId planetId)
        {
            if (!_runManager.HasActiveRun || planetId.IsEmpty)
            {
                return;
            }

            var result = _sensor.ScanPlanet(_runManager.CurrentRun, ObserverId.Player, planetId);
            if (!result.Success)
            {
                _viewBridge.SetStatus(result.Error.Message);
                return;
            }

            _selectedPlanetId = planetId;
            _viewBridge.SetStatus("Scanned " + planetId.Value + ".");
        }

        private void EnterStarSystemView(RunState run, bool keepSelectedPlanet)
        {
            if (!run.Universe.MapGraph.TryGetNode(_selectedNodeId, out var node))
            {
                _inStarSystemView = false;
                _viewBridge.ShowStarMap();
                RefreshInspection(run);
                return;
            }

            var visiblePlanets = _query.VisiblePlanets(run, node);
            if (!keepSelectedPlanet
                || _selectedPlanetId.IsEmpty
                || !ContainsPlanet(visiblePlanets, _selectedPlanetId))
            {
                _selectedPlanetId = default;
            }

            _inStarSystemView = true;
            _viewBridge.ShowStarSystemView(_query.NodeLabel(node), visiblePlanets);
            RefreshSystemPlanetStates(run);
            RefreshInspection(run);
            _viewBridge.SetStatus(_query.StarSystemStatus(visiblePlanets.Count));
        }

        private void RefreshSystemPlanetStates(RunState run)
        {
            if (!run.Universe.MapGraph.TryGetNode(_selectedNodeId, out var node))
            {
                return;
            }

            for (var i = 0; i < node.PlanetIds.Count; i++)
            {
                var planetId = node.PlanetIds[i];
                if (!_viewBridge.StarSystemView.HasPlanet(planetId))
                {
                    continue;
                }

                _viewBridge.StarSystemView.SetPlanetState(
                    planetId,
                    _query.PlanetState(run, node, planetId, _selectedPlanetId));
            }
        }

        private void RefreshRoutes(RunState run)
        {
            _viewBridge.StarMap.SetRoutes(_query.KnownRoutes(run));
        }

        private void RefreshNodeStates(RunState run)
        {
            foreach (var node in run.Universe.MapGraph.Nodes)
            {
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                _viewBridge.StarMap.SetNodeState(node.Id, _query.NodeState(run, node, _selectedNodeId));
            }
        }

        private void RefreshInspection(RunState run)
        {
            var inspection = _query.DescribeSelection(run, _selectedNodeId, _selectedPlanetId, _inStarSystemView);
            if (!inspection.HasSelection)
            {
                _viewBridge.StarSystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            _viewBridge.StarSystemInspection.ShowNode(inspection.Title, inspection.Summary, inspection.CanTravel);
            if (inspection.ShowPlanetDetail)
            {
                _viewBridge.PlanetInspection.ShowPlanet(inspection.PlanetDetail);
            }
            else
            {
                _viewBridge.PlanetInspection.Clear();
            }
        }

        private void OnFleetTravel(FleetTravelEvent domainEvent)
        {
            RefreshFleetPresence();
            _viewBridge.SetStatus(
                domainEvent.Kind == FleetTravelKind.Arrived
                    ? "Fleet arrived."
                    : "Fleet is traveling.");
        }

        private void OnMapKnowledgeChanged(MapKnowledgeChangedEvent domainEvent)
        {
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            if (domainEvent.Kind == MapKnowledgeKind.PlanetScanned)
            {
                if (!_inStarSystemView)
                {
                    return;
                }

                RefreshSystemPlanetStates(run);
                RefreshInspection(run);
                return;
            }

            RefreshRoutes(run);
            RefreshNodeStates(run);
            RefreshInspection(run);
        }

        private void RefreshFleetPresence()
        {
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            RefreshNodeStates(run);
            RefreshInspection(run);
        }

        private static bool ContainsPlanet(IReadOnlyList<ContentId> planets, ContentId planetId)
        {
            for (var i = 0; i < planets.Count; i++)
            {
                if (planets[i] == planetId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
