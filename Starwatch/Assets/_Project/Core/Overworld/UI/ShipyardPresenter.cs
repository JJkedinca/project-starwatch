using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Fleet;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Shipyard intents for the selected colony.
    /// Queue progress is the exception to event-only presenters: a domain event is journaled,
    /// so the bar samples the clock about once a second while a queue is visible.
    /// </summary>
    internal sealed class ShipyardPresenter : IStartable, IDisposable
    {
        private const float kRefreshSeconds = 1f;

        private readonly IShipyardView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly ShipProductionService _shipProductionService;
        private readonly ShipyardQuery _query;
        private readonly SimulationClock _clock;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private bool _queueActive;
        private double _nextRefresh;

        public ShipyardPresenter(
            IShipyardView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            ShipProductionService shipProductionService,
            ShipyardQuery query,
            SimulationClock clock,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _shipProductionService = shipProductionService ?? throw new ArgumentNullException(nameof(shipProductionService));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.QueueRequested += OnQueueRequested;
            _view.CancelRequested += OnCancelRequested;
            _selection.Changed += OnSelectionChanged;
            _clock.TimeAdvanced += OnTimeAdvanced;
            _subscriptions.Add(_events.Subscribe<ShipQueuedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResourceChangedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<BuildingChangedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<SettlementFoundedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<SettlementAbandonedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _view.QueueRequested -= OnQueueRequested;
            _view.CancelRequested -= OnCancelRequested;
            _selection.Changed -= OnSelectionChanged;
            _clock.TimeAdvanced -= OnTimeAdvanced;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnQueueRequested(ContentId classId)
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedPlanetId.IsEmpty)
            {
                return;
            }

            var result = _shipProductionService.Queue(run, _selection.SelectedPlanetId, classId);
            _status.PostCommand(result.Success, "Ship queued.", result.Error.Message);
        }

        private void OnCancelRequested(int queueIndex)
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedPlanetId.IsEmpty)
            {
                return;
            }

            var result = _shipProductionService.Cancel(run, _selection.SelectedPlanetId, queueIndex);
            if (!result.Success)
            {
                _status.Post(result.Error.Message, StatusKind.Error);
                return;
            }

            _status.Post("Build cancelled.", StatusKind.Result);
            Apply();
        }

        private void OnSelectionChanged()
        {
            Apply();
        }

        private void OnTimeAdvanced(GameTime time, float deltaSeconds)
        {
            if (!_queueActive || deltaSeconds <= 0f || time.Seconds + 0.0001d < _nextRefresh)
            {
                return;
            }

            _nextRefresh = time.Seconds + kRefreshSeconds;
            Apply();
        }

        private void Apply()
        {
            var model = _query.Describe(CurrentRun(), _selection);
            var active = model.IsShown && model.Queue.Count > 0;
            if (active && !_queueActive)
            {
                _nextRefresh = _clock.CurrentTime.Seconds + kRefreshSeconds;
            }

            _queueActive = active;
            _view.Apply(model);
        }

        private RunState CurrentRun()
        {
            return _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
