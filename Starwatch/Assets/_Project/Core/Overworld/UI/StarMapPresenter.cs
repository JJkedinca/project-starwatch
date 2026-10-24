using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld.Services;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Star map intents: select a node, enter a system, travel, and hover course preview.
    /// Start paints the whole map. Later events repaint only the nodes they change.
    /// Hover never writes to <see cref="OverworldSelection"/>.
    /// </summary>
    internal sealed class StarMapPresenter : IStartable, IDisposable
    {
        private readonly IStarMapView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly TravelService _travelService;
        private readonly StarMapQuery _query;
        private readonly StarSystemQuery _systemQuery;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<StarMapNodeModel> _nodes = new();
        private ContentId _selectedNodeId;
        private ContentId _hoverNodeId;
        private bool _inStarSystem;

        public StarMapPresenter(
            IStarMapView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            TravelService travelService,
            StarMapQuery query,
            StarSystemQuery systemQuery,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _travelService = travelService ?? throw new ArgumentNullException(nameof(travelService));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _systemQuery = systemQuery ?? throw new ArgumentNullException(nameof(systemQuery));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.NodeSelected += OnNodeSelected;
            _view.TravelRequested += OnTravelRequested;
            _view.NodeHovered += OnNodeHovered;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(OnFleetTravel));
            _subscriptions.Add(_events.Subscribe<FleetCreatedEvent>(OnFleetCreated));
            _subscriptions.Add(_events.Subscribe<FleetMergedEvent>(_ => RefreshCardAndRoutes()));
            _subscriptions.Add(_events.Subscribe<FleetSplitEvent>(_ => RefreshCardAndRoutes()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => RefreshCardAndRoutes()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => RefreshCardAndRoutes()));
            _subscriptions.Add(_events.Subscribe<RewindCompletedEvent>(OnRewind));
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            if (!_runManager.HasActiveRun)
            {
                _status.Post(OverworldStatus.kNoRun, StatusKind.Result);
                _selection.Clear();
            }

            Apply();
            _selectedNodeId = _selection.SelectedNodeId;
            _inStarSystem = _selection.InStarSystem;
        }

        public void Dispose()
        {
            _view.NodeSelected -= OnNodeSelected;
            _view.TravelRequested -= OnTravelRequested;
            _view.NodeHovered -= OnNodeHovered;
            _selection.Changed -= OnSelectionChanged;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnNodeSelected(ContentId nodeId)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            var wasInSystem = _selection.InStarSystem;
            var previousNode = _selection.SelectedNodeId;
            if (_query.CanEnter(run, nodeId))
            {
                ClearHover();
                _selection.EnterSystem(nodeId);
                if (!wasInSystem || previousNode != nodeId)
                {
                    _status.Post(_systemQuery.Describe(run, _selection).Status, StatusKind.Result);
                }

                return;
            }

            _selection.SelectNode(nodeId);
        }

        private void OnNodeHovered(ContentId nodeId)
        {
            _hoverNodeId = nodeId;
            RefreshHover();
        }

        private void ClearHover()
        {
            if (_hoverNodeId.IsEmpty)
            {
                return;
            }

            _hoverNodeId = default;
            RefreshHover();
        }

        private void OnRewind(RewindCompletedEvent _)
        {
            ClearHover();
            if (!_selection.SelectedFleetId.IsNone)
            {
                _selection.SelectFleet(EntityId.None);
            }

            // Timeline state changed under the map; repaint nodes and routes even if
            // selection did not move (e.g. traveling markers, in-transit lines).
            Apply();
            _selectedNodeId = _selection.SelectedNodeId;
            _view.ApplyHoverTooltip(StarMapHoverTooltipModel.Hidden);
        }

        private void OnTravelRequested(ContentId destinationNodeId)
        {
            var run = CurrentRun();
            if (run == null || destinationNodeId.IsEmpty)
            {
                return;
            }

            if (_selection.SelectedFleetId.IsNone)
            {
                return;
            }

            var result = _travelService.StartTravel(
                run,
                ObserverId.Player,
                _selection.SelectedFleetId,
                destinationNodeId);
            if (!result.Success)
            {
                _status.Post(result.Error.Message, StatusKind.Error);
            }
        }

        private void OnFleetCreated(FleetCreatedEvent domainEvent)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            _nodes.Clear();
            _query.AppendNodeAndNeighbors(run, domainEvent.LocationId, _selection.SelectedNodeId, _nodes);
            _view.ApplyNodes(_nodes);
            RefreshCardAndRoutes();
        }

        private void OnFleetTravel(FleetTravelEvent domainEvent)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            _nodes.Clear();
            var selected = _selection.SelectedNodeId;
            if (domainEvent.Kind == FleetTravelKind.Arrived)
            {
                _query.AppendNodeAndNeighbors(run, domainEvent.FromNodeId, selected, _nodes);
                _query.AppendNodeAndNeighbors(run, domainEvent.ToNodeId, selected, _nodes);
            }
            else
            {
                _query.AppendNode(run, domainEvent.FromNodeId, selected, _nodes);
                _query.AppendNode(run, domainEvent.ToNodeId, selected, _nodes);
            }

            _view.ApplyNodes(_nodes);
            RefreshCardAndRoutes();
            if (domainEvent.Kind == FleetTravelKind.Started)
            {
                _status.Post("Fleet is traveling.", StatusKind.Result);
            }
        }

        private void OnMapKnowledgeChanged(MapKnowledgeChangedEvent domainEvent)
        {
            if (domainEvent.ObserverId != ObserverId.Player
                || domainEvent.Kind != MapKnowledgeKind.NodeExplored)
            {
                return;
            }

            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            RefreshRoutes(run);
            _nodes.Clear();
            _query.AppendNodeAndNeighbors(run, domainEvent.SubjectId, _selection.SelectedNodeId, _nodes);
            _view.ApplyNodes(_nodes);
        }

        private void OnSelectionChanged()
        {
            var inSystem = _selection.InStarSystem;
            if (inSystem != _inStarSystem)
            {
                _inStarSystem = inSystem;
                Apply();
                _selectedNodeId = _selection.SelectedNodeId;
                return;
            }

            var run = CurrentRun();
            var current = _selection.SelectedNodeId;
            if (run != null && current != _selectedNodeId)
            {
                _nodes.Clear();
                _query.AppendNode(run, _selectedNodeId, current, _nodes);
                _query.AppendNode(run, current, current, _nodes);
                _view.ApplyNodes(_nodes);
            }

            _selectedNodeId = current;
            if (run == null)
            {
                return;
            }

            _view.ApplyCard(_query.DescribeCard(run, _selection));
            RefreshFleetMarkers();
            RefreshHover();
        }

        private void RefreshCardAndRoutes()
        {
            if (ReleaseMissingFleet())
            {
                return;
            }

            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            _view.ApplyCard(_query.DescribeCard(run, _selection));
            RefreshRoutes(run);
            RefreshFleetMarkers();
            RefreshHoverTooltip(run);
        }

        private void RefreshFleetMarkers()
        {
            var run = CurrentRun();
            if (run == null)
            {
                _view.ApplyFleetMarkers(Array.Empty<FleetMarkerModel>());
                return;
            }

            _view.ApplyFleetMarkers(_query.DescribeFleetMarkers(run, _selection));
        }

        private void RefreshHover()
        {
            var run = CurrentRun();
            if (run == null)
            {
                _view.ApplyHoverTooltip(StarMapHoverTooltipModel.Hidden);
                return;
            }

            RefreshRoutes(run);
            RefreshHoverTooltip(run);
        }

        private void RefreshRoutes(RunState run)
        {
            _view.ApplyRoutes(_query.DescribeRoutes(run, _selection, _hoverNodeId));
        }

        private void RefreshHoverTooltip(RunState run)
        {
            _view.ApplyHoverTooltip(_query.DescribeHoverTooltip(run, _selection, _hoverNodeId));
        }

        private bool ReleaseMissingFleet()
        {
            var selected = _selection.SelectedFleetId;
            if (selected.IsNone)
            {
                return false;
            }

            var run = CurrentRun();
            if (run != null && run.Timeline.Fleets.TryGetFleet(selected, out _))
            {
                return false;
            }

            _selection.SelectFleet(EntityId.None);
            return true;
        }

        private void Apply()
        {
            _view.Apply(_query.Describe(CurrentRun(), _selection));
        }

        private RunState CurrentRun()
        {
            return _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
