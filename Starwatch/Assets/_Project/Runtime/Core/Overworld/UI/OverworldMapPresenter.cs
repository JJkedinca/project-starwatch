using System;
using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld.Services;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Overworld map presenter. Binds UI intents to explore/scan services and refreshes views.
    /// Fleet travel controls are deferred; known routes are drawn from archive knowledge.
    /// TODO: Extract OverworldMapQuery (read-model/labels/planet rows) when travel UI lands;
    /// keep this type as intent routing + refresh only. See TODOs.md.
    /// </summary>
    public sealed class OverworldMapPresenter : IStartable
    {
        private readonly OverworldViewBridge _viewBridge;
        private readonly RunManager _runManager;
        private readonly VisibilityService _visibility;
        private readonly ExplorationService _exploration;
        private readonly SensorService _sensor;
        private readonly ContentCatalog _contentCatalog;

        private ContentId _selectedNodeId;
        private ContentId _selectedPlanetId;
        private bool _inSystemView;

        public OverworldMapPresenter(
            OverworldViewBridge viewBridge,
            RunManager runManager,
            VisibilityService visibility,
            ExplorationService exploration,
            SensorService sensor,
            ContentCatalog contentCatalog)
        {
            _viewBridge = viewBridge ?? throw new ArgumentNullException(nameof(viewBridge));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            _exploration = exploration ?? throw new ArgumentNullException(nameof(exploration));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _contentCatalog = contentCatalog ?? throw new ArgumentNullException(nameof(contentCatalog));
        }

        /// <summary>
        /// Binds UI events and builds the initial map from the active run.
        /// </summary>
        public void Start()
        {
            _viewBridge.NodeSelected += OnNodeSelected;
            _viewBridge.ExploreRequested += OnExploreRequested;
            _viewBridge.ScanPlanetRequested += OnScanPlanetRequested;
            _viewBridge.SystemBackRequested += OnSystemBackRequested;
            _viewBridge.PlanetSelected += OnPlanetSelected;
            _viewBridge.Bind();
            RefreshAll();
        }

        /// <summary>
        /// Rebuilds map nodes and inspection from the active run.
        /// </summary>
        private void RefreshAll()
        {
            if (!_runManager.HasActiveRun)
            {
                _inSystemView = false;
                _selectedPlanetId = default;
                _viewBridge.ShowStarMap();
                _viewBridge.StarMap.Clear();
                _viewBridge.SystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                _viewBridge.SetStatus("No active run.");
                return;
            }

            var run = _runManager.CurrentRun;
            _viewBridge.StarMap.Rebuild(run.Universe.MapGraph.Nodes);
            RefreshRoutes(run);
            RefreshNodeStates(run);

            if (_inSystemView)
            {
                EnterSystemView(run, keepSelectedPlanet: true);
            }
            else
            {
                _viewBridge.ShowStarMap();
                RefreshInspection(run);
            }

            _viewBridge.SetStatus(
                _inSystemView
                    ? "Select a planet to inspect or scan."
                    : "Explore adjacent nodes, then open a system to scan planets.");
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

            if (CanEnterSystemView(run, nodeId))
            {
                EnterSystemView(run, keepSelectedPlanet: false);
                return;
            }

            _inSystemView = false;
            _viewBridge.ShowStarMap();
            RefreshInspection(run);
        }

        private void OnSystemBackRequested()
        {
            _inSystemView = false;
            _selectedPlanetId = default;
            _viewBridge.ShowStarMap();
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            RefreshNodeStates(_runManager.CurrentRun);
            RefreshInspection(_runManager.CurrentRun);
            _viewBridge.SetStatus("Explore adjacent nodes, then open a system to scan planets.");
        }

        private void OnPlanetSelected(ContentId planetId)
        {
            _selectedPlanetId = planetId;
            if (!_runManager.HasActiveRun || !_inSystemView)
            {
                return;
            }

            var run = _runManager.CurrentRun;
            RefreshSystemPlanetStates(run);
            RefreshInspection(run);
        }

        private void OnExploreRequested()
        {
            if (!_runManager.HasActiveRun || _selectedNodeId.IsEmpty)
            {
                return;
            }

            var result = _exploration.Explore(
                _runManager.CurrentRun,
                ObserverId.Player,
                _selectedNodeId);
            if (!result.Success)
            {
                _viewBridge.SetStatus(result.Error.Message);
                return;
            }

            RefreshAll();
            if (_runManager.HasActiveRun && CanEnterSystemView(_runManager.CurrentRun, _selectedNodeId))
            {
                EnterSystemView(_runManager.CurrentRun, keepSelectedPlanet: false);
            }

            _viewBridge.SetStatus("Explored " + _selectedNodeId.Value + ".");
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

        private void EnterSystemView(RunState run, bool keepSelectedPlanet)
        {
            if (!run.Universe.MapGraph.TryGetNode(_selectedNodeId, out var node))
            {
                _inSystemView = false;
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

            _inSystemView = true;
            _viewBridge.ShowSystemView(FormatNodeLabel(node), visiblePlanets);
            RefreshSystemPlanetStates(run);
            RefreshInspection(run);
            _viewBridge.SetStatus("Select a planet to inspect or scan.");
        }

        private static bool CanEnterSystemView(RunState run, ContentId nodeId)
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
                if (!_viewBridge.SystemView.HasPlanet(planetId))
                {
                    continue;
                }

                var visibility = _visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    node.Id,
                    planetId);
                var displayName = planetId.Value;
                if (_contentCatalog.TryGetPlanet(planetId.Value, out var definition))
                {
                    displayName = definition.DisplayName;
                }

                _viewBridge.SystemView.SetPlanetState(
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
                var state = new MapNodePresentationState(
                    visibility,
                    isExplorable: _exploration.CanExplore(run, ObserverId.Player, node.Id),
                    isSelected: node.Id == _selectedNodeId,
                    hasFleet: visibility == MapNodeVisibility.Present,
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
                _viewBridge.SystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var visibility = _visibility.GetNodeVisibility(
                run.Archive,
                run.Timeline,
                ObserverId.Player,
                node.Id);
            var canExplore = _exploration.CanExplore(run, ObserverId.Player, node.Id);

            if (visibility == MapNodeVisibility.Hidden)
            {
                _viewBridge.SystemInspection.ShowNode(
                    canExplore ? "Unknown signal" : "Uncharted space",
                    canExplore
                        ? "Adjacent to known space. Explore to reveal this node."
                        : "Hidden. Explore neighboring systems first.",
                    canExplore);
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            var kindLabel = node.Kind == MapNodeKind.System ? "System" : "Non-system node";
            var summary = kindLabel + " · " + node.Id.Value;
            if (visibility == MapNodeVisibility.Present)
            {
                summary += "\nFleet present.";
            }

            if (canExplore)
            {
                summary += "\nExplore to survey this system.";
            }
            else if (node.Kind == MapNodeKind.System && !_inSystemView)
            {
                summary += "\nOpen to inspect planets.";
            }

            _viewBridge.SystemInspection.ShowNode(FormatNodeLabel(node), summary, canExplore);
            RefreshSelectedPlanetDetail(run, node);
        }

        private void RefreshSelectedPlanetDetail(RunState run, MapNode node)
        {
            if (!_inSystemView || _selectedPlanetId.IsEmpty)
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

            if (_contentCatalog.TryGetPlanet(_selectedPlanetId.Value, out var definition))
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
