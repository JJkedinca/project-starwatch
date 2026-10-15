using System;
using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Overworld map presenter. Binds UI intents to travel and scan, then refreshes views.
    /// Registered on the simulation loop after travel resolution so fleet markers follow arrivals.
    /// TODO: Extract OverworldMapQuery (read-model/labels/planet rows) if this type keeps growing.
    /// </summary>
    public sealed class OverworldMapPresenter : IStartable, IDisposable, ISimulationSystem
    {
        private readonly OverworldViewBridge _viewBridge;
        private readonly RunManager _runManager;
        private readonly VisibilityService _visibility;
        private readonly SensorService _sensor;
        private readonly TravelService _travel;
        private readonly SimulationLoop _simulationLoop;
        private readonly ObjectDatabaseSO _objectDatabase;

        private ContentId _selectedNodeId;
        private ContentId _selectedPlanetId;
        private bool _inStarSystemView;
        private int _fleetRevision;

        public OverworldMapPresenter(
            OverworldViewBridge viewBridge,
            RunManager runManager,
            VisibilityService visibility,
            SensorService sensor,
            TravelService travel,
            SimulationLoop simulationLoop,
            ObjectDatabaseSO objectDatabase)
        {
            _viewBridge = viewBridge ?? throw new ArgumentNullException(nameof(viewBridge));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _simulationLoop = simulationLoop ?? throw new ArgumentNullException(nameof(simulationLoop));
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
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
            _simulationLoop.Register(this);
            RefreshAll();
        }

        /// <summary>
        /// Refreshes fleet markers after TravelSystem resolves arrivals on this tick.
        /// </summary>
        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            _ = time;
            _ = tickDeltaSeconds;
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            var revision = ComputeFleetRevision(run.Timeline);
            if (revision == _fleetRevision)
            {
                return;
            }

            _fleetRevision = revision;
            RefreshNodeStates(run);
            RefreshInspection(run);
        }

        /// <summary>
        /// Drops the simulation registration when the overworld scope ends.
        /// </summary>
        public void Dispose()
        {
            _simulationLoop.Unregister(this);
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
            }
            else
            {
                _viewBridge.ShowStarMap();
                RefreshInspection(run);
            }

            _fleetRevision = ComputeFleetRevision(run.Timeline);
            _viewBridge.SetStatus(
                _inStarSystemView
                    ? "Select a planet to inspect or scan."
                    : "Travel a known route to enter a star system, then scan its planets.");
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

            if (CanEnterStarSystemView(run, nodeId))
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
            if (!TryFindFleetForDestination(run, _selectedNodeId, out var fleetId))
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

            _fleetRevision = ComputeFleetRevision(run.Timeline);
            RefreshNodeStates(run);
            RefreshInspection(run);
            _viewBridge.SetStatus("Fleet is traveling.");
        }

        private void OnScanPlanetRequested(ContentId planetId)
        {
            if (!_runManager.HasActiveRun || planetId.IsEmpty)
            {
                return;
            }

            var result = _sensor.ScanPlanet(
                _runManager.CurrentRun,
                ObserverId.Player,
                planetId);
            if (!result.Success)
            {
                _viewBridge.SetStatus(result.Error.Message);
                return;
            }

            _selectedPlanetId = planetId;
            RefreshAll();
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

            var visiblePlanets = CollectVisiblePlanets(run, node);
            if (!keepSelectedPlanet
                || _selectedPlanetId.IsEmpty
                || !ContainsPlanet(visiblePlanets, _selectedPlanetId))
            {
                _selectedPlanetId = default;
            }

            _inStarSystemView = true;
            _viewBridge.ShowStarSystemView(FormatNodeLabel(node), visiblePlanets);
            RefreshSystemPlanetStates(run);
            RefreshInspection(run);
            _viewBridge.SetStatus("Select a planet to inspect or scan.");
        }

        private static bool CanEnterStarSystemView(RunState run, ContentId nodeId)
        {
            if (nodeId.IsEmpty || !run.Universe.MapGraph.TryGetNode(nodeId, out var node))
            {
                return false;
            }

            if (node.Kind != MapNodeKind.System)
            {
                return false;
            }

            return run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge)
                && knowledge.HasExploredNode(nodeId);
        }

        private List<ContentId> CollectVisiblePlanets(RunState run, MapNode node)
        {
            List<ContentId> planets = new();
            for (var i = 0; i < node.PlanetIds.Count; i++)
            {
                var planetId = node.PlanetIds[i];
                var visibility = _visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    node.Id,
                    planetId);
                if (visibility == PlanetVisibility.Hidden)
                {
                    continue;
                }

                planets.Add(planetId);
            }

            return planets;
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

                var visibility = _visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    node.Id,
                    planetId);
                var displayName = planetId.Value;
                if (_objectDatabase.TryGetPlanet(planetId.Value, out var definition))
                {
                    displayName = definition.DisplayName;
                }

                _viewBridge.StarSystemView.SetPlanetState(
                    planetId,
                    new PlanetPresentationState(
                        visibility,
                        isSelected: planetId == _selectedPlanetId,
                        displayName));
            }
        }

        private void RefreshRoutes(RunState run)
        {
            if (!run.Archive.TryGetMapKnowledge(ObserverId.Player, out var knowledge))
            {
                _viewBridge.StarMap.SetRoutes(Array.Empty<MapRouteSegment>());
                return;
            }

            var graph = run.Universe.MapGraph;
            List<MapRouteSegment> segments = new();
            for (var i = 0; i < graph.Routes.Count; i++)
            {
                var route = graph.Routes[i];
                if (route == null)
                {
                    continue;
                }

                var key = RouteService.MakeRouteKey(route.NodeAId, route.NodeBId);
                if (!knowledge.HasKnownRoute(key))
                {
                    continue;
                }

                if (!graph.TryGetNode(route.NodeAId, out var nodeA)
                    || !graph.TryGetNode(route.NodeBId, out var nodeB))
                {
                    continue;
                }

                segments.Add(new MapRouteSegment(nodeA.MapPosition, nodeB.MapPosition));
            }

            _viewBridge.StarMap.SetRoutes(segments);
        }

        private void RefreshNodeStates(RunState run)
        {
            foreach (var node in run.Universe.MapGraph.Nodes)
            {
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                var visibility = _visibility.GetNodeVisibility(
                    run.Archive,
                    run.Timeline,
                    ObserverId.Player,
                    node.Id);
                var hasFleet = visibility != MapNodeVisibility.Hidden
                    && NodeHasFleet(run.Timeline, node.Id);
                var state = new MapNodePresentationState(
                    visibility,
                    isExplorable: CanTravelTo(run, node.Id),
                    isSelected: node.Id == _selectedNodeId,
                    hasFleet,
                    isFleetTraveling: hasFleet && NodeFleetIsTraveling(run.Timeline, node.Id),
                    revealedLabel: visibility == MapNodeVisibility.Hidden
                        ? string.Empty
                        : FormatNodeLabel(node));

                _viewBridge.StarMap.SetNodeState(node.Id, state);
            }
        }

        private void RefreshInspection(RunState run)
        {
            if (_selectedNodeId.IsEmpty
                || !run.Universe.MapGraph.TryGetNode(_selectedNodeId, out var node))
            {
                _viewBridge.StarSystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var visibility = _visibility.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var canTravel = CanTravelTo(run, node.Id);

            if (visibility == MapNodeVisibility.Hidden)
            {
                _viewBridge.StarSystemInspection.ShowNode(
                    "Uncharted space",
                    canTravel
                        ? "Known route. Travel here to enter."
                        : "Hidden until a fleet arrives by a known route.",
                    canTravel);
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var kindLabel = node.Kind == MapNodeKind.System ? "Star system" : "Non-system node";
            var summary = kindLabel + " · " + node.Id.Value;
            if (visibility == MapNodeVisibility.Present)
            {
                summary += "\nFleet present.";
            }

            if (node.Kind == MapNodeKind.System && !_inStarSystemView)
            {
                summary += "\nOpen to inspect planets.";
            }

            _viewBridge.StarSystemInspection.ShowNode(
                FormatNodeLabel(node),
                summary,
                canTravel);
            RefreshSelectedPlanetDetail(run, node);
        }

        private void RefreshSelectedPlanetDetail(RunState run, MapNode node)
        {
            if (!_inStarSystemView || _selectedPlanetId.IsEmpty)
            {
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var planetVisibility = _visibility.GetPlanetVisibility(
                run.Archive,
                ObserverId.Player,
                node.Id,
                _selectedPlanetId);
            if (planetVisibility == PlanetVisibility.Hidden)
            {
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var displayName = _selectedPlanetId.Value;
            var detail = "Ready to scan.";
            var canScan = planetVisibility == PlanetVisibility.SystemExplored;

            if (_objectDatabase.TryGetPlanet(_selectedPlanetId.Value, out var definition))
            {
                displayName = definition.DisplayName;
                if (planetVisibility == PlanetVisibility.Scanned)
                {
                    detail = definition.Summary;
                }
            }
            else if (planetVisibility == PlanetVisibility.Scanned)
            {
                detail = "Scanned.";
            }

            _viewBridge.PlanetInspection.ShowPlanet(
                new PlanetDetailModel(_selectedPlanetId, displayName, detail, canScan));
        }

        private bool CanTravelTo(RunState run, ContentId destinationNodeId)
        {
            return TryFindFleetForDestination(run, destinationNodeId, out _);
        }

        private bool TryFindFleetForDestination(RunState run, ContentId destinationNodeId, out EntityId fleetId)
        {
            fleetId = EntityId.None;
            var fleets = run.Timeline.Fleets;
            for (var i = 0; i < fleets.Count; i++)
            {
                if (_travel.CanStartTravel(run, ObserverId.Player, fleets[i].Id, destinationNodeId))
                {
                    fleetId = fleets[i].Id;
                    return true;
                }
            }

            return false;
        }

        private static bool NodeHasFleet(TimelineState timeline, ContentId nodeId)
        {
            for (var i = 0; i < timeline.Fleets.Count; i++)
            {
                if (timeline.Fleets[i].LocationSystemId == nodeId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool NodeFleetIsTraveling(TimelineState timeline, ContentId nodeId)
        {
            for (var i = 0; i < timeline.Fleets.Count; i++)
            {
                var fleet = timeline.Fleets[i];
                if (fleet.LocationSystemId == nodeId && timeline.IsFleetTraveling(fleet.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private static int ComputeFleetRevision(TimelineState timeline)
        {
            var revision = timeline.ActiveTravels.Count;
            for (var i = 0; i < timeline.Fleets.Count; i++)
            {
                var fleet = timeline.Fleets[i];
                revision = unchecked(revision * 397) ^ fleet.LocationSystemId.GetHashCode();
                revision = unchecked(revision * 397) ^ (timeline.IsFleetTraveling(fleet.Id) ? 1 : 0);
            }

            return revision;
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

        private static string FormatNodeLabel(MapNode node)
        {
            return node.Id.Value;
        }
    }
}
