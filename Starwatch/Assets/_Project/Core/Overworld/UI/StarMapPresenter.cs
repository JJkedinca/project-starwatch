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
    /// Star map intents: select a node, enter a system, and travel.
    /// Start paints the whole map. Later events repaint only the nodes they change.
    /// </summary>
    internal sealed class StarMapPresenter : IStartable, IDisposable
    {
        private readonly IStarMapView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly TravelService _travel;
        private readonly StarMapQuery _query;
        private readonly StarSystemQuery _systemQuery;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<StarMapNodeModel> _nodes = new();
        private ContentId _selectedNodeId;

        public StarMapPresenter(
            IStarMapView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            TravelService travel,
            StarMapQuery query,
            StarSystemQuery systemQuery,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _travel = travel ?? throw new ArgumentNullException(nameof(travel));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _systemQuery = systemQuery ?? throw new ArgumentNullException(nameof(systemQuery));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.NodeSelected += OnNodeSelected;
            _view.TravelRequested += OnTravelRequested;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(OnFleetTravel));
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            if (!_runManager.HasActiveRun)
            {
                _status.Post(OverworldStatus.kNoRun);
                _selection.Clear();
            }
            else if (!_selection.InStarSystem)
            {
                _status.Post(OverworldStatus.kMapHint);
            }

            Apply();
            _selectedNodeId = _selection.SelectedNodeId;
        }

        public void Dispose()
        {
            _view.NodeSelected -= OnNodeSelected;
            _view.TravelRequested -= OnTravelRequested;
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
                _selection.EnterSystem(nodeId);
                if (!wasInSystem || previousNode != nodeId)
                {
                    _status.Post(_systemQuery.Describe(run, _selection).Status);
                }

                return;
            }

            _selection.SelectNode(nodeId);
            if (wasInSystem)
            {
                _status.Post(OverworldStatus.kMapHint);
            }
        }

        private void OnTravelRequested()
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedNodeId.IsEmpty)
            {
                return;
            }

            if (!_query.TryFindFleetForDestination(run, _selection.SelectedNodeId, out var fleetId))
            {
                _status.Post("Travel is not available.");
                return;
            }

            var result = _travel.StartTravel(run, ObserverId.Player, fleetId, _selection.SelectedNodeId);
            if (!result.Success)
            {
                _status.Post(result.Error.Message);
            }
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
            _view.ApplyCard(_query.DescribeCard(run, _selection));
            _status.Post(domainEvent.Kind == FleetTravelKind.Arrived ? "Fleet arrived." : "Fleet is traveling.");
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

            _view.ApplyRoutes(_query.KnownRoutes(run));
            _nodes.Clear();
            _query.AppendNodeAndNeighbors(run, domainEvent.SubjectId, _selection.SelectedNodeId, _nodes);
            _view.ApplyNodes(_nodes);
        }

        private void OnSelectionChanged()
        {
            var run = CurrentRun();
            var current = _selection.SelectedNodeId;
            if (run == null)
            {
                _selectedNodeId = current;
                return;
            }

            _nodes.Clear();
            _query.AppendNode(run, _selectedNodeId, current, _nodes);
            _query.AppendNode(run, current, current, _nodes);
            _view.ApplyNodes(_nodes);
            _view.ApplyCard(_query.DescribeCard(run, _selection));
            _selectedNodeId = current;
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
