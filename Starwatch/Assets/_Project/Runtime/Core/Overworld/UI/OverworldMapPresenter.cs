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
    /// Travel controls are deferred to presentation Step 2.
    /// TODO: Extract OverworldMapQuery (read-model/labels/planet rows) when Step 2 UI lands;
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
                _viewBridge.StarMap.Clear();
                _viewBridge.SystemInspection.ShowEmpty();
                _viewBridge.PlanetInspection.Clear();
                _viewBridge.SetStatus("No active run.");
                return;
            }

            var run = _runManager.CurrentRun;
            _viewBridge.StarMap.Rebuild(run.Universe.MapGraph.Nodes);
            RefreshNodeStates(run);
            RefreshInspection(run);
            _viewBridge.SetStatus("Explore adjacent nodes, then scan planets.");
        }

        private void OnNodeSelected(ContentId nodeId)
        {
            _selectedNodeId = nodeId;
            if (!_runManager.HasActiveRun)
            {
                return;
            }

            RefreshNodeStates(_runManager.CurrentRun);
            RefreshInspection(_runManager.CurrentRun);
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

            RefreshAll();
            _viewBridge.SetStatus("Scanned " + planetId.Value + ".");
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

            _viewBridge.SystemInspection.ShowNode(FormatNodeLabel(node), summary, canExplore);
            RefreshPlanets(run, node);
        }

        private void RefreshPlanets(RunState run, MapNode node)
        {
            if (node.PlanetIds.Count == 0)
            {
                _viewBridge.PlanetInspection.Clear();
                return;
            }

            List<PlanetRowModel> rows = new();
            for (var i = 0; i < node.PlanetIds.Count; i++)
            {
                var planetId = node.PlanetIds[i];
                var planetVisibility = _visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    node.Id,
                    planetId);

                // Hidden planets stay out of the panel until the host system is explored.
                if (planetVisibility == PlanetVisibility.Hidden)
                {
                    continue;
                }

                var displayName = planetId.Value;
                var detail = "Ready to scan.";
                var canScan = planetVisibility == PlanetVisibility.SystemExplored;

                if (_contentCatalog.TryGetPlanet(planetId.Value, out var definition))
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

                rows.Add(new PlanetRowModel(planetId, displayName, detail, canScan));
            }

            _viewBridge.PlanetInspection.ShowPlanets(rows);
        }

        private static string FormatNodeLabel(MapNode node)
        {
            return node.Id.Value;
        }
    }
}
