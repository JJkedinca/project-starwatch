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
    /// Drives the selected-fleet bar and the right-column details panel.
    /// Transit ETAs sample the clock about once a second while the selected fleet is traveling.
    /// </summary>
    internal sealed class FleetPanelPresenter : IStartable, IDisposable
    {
        private readonly IFleetPanelView _detailsView;
        private readonly IFleetBarView _barView;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly FleetService _fleetService;
        private readonly FleetPanelQuery _query;
        private readonly SimulationClock _clock;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private readonly TransitCountdown _countdown = new();

        public FleetPanelPresenter(
            IFleetPanelView detailsView,
            IFleetBarView barView,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            FleetService fleetService,
            FleetPanelQuery query,
            SimulationClock clock,
            DomainEventBus events)
        {
            _detailsView = detailsView ?? throw new ArgumentNullException(nameof(detailsView));
            _barView = barView ?? throw new ArgumentNullException(nameof(barView));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _fleetService = fleetService ?? throw new ArgumentNullException(nameof(fleetService));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _detailsView.CloseRequested += OnDetailsCloseRequested;
            _detailsView.SplitRequested += OnSplitRequested;
            _detailsView.MergeRequested += OnMergeRequested;
            _barView.DetailsRequested += OnDetailsRequested;
            _barView.CloseRequested += OnBarCloseRequested;
            _selection.Changed += OnSelectionChanged;
            _clock.TimeAdvanced += OnTimeAdvanced;
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetCreatedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetMergedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetSplitEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _detailsView.CloseRequested -= OnDetailsCloseRequested;
            _detailsView.SplitRequested -= OnSplitRequested;
            _detailsView.MergeRequested -= OnMergeRequested;
            _barView.DetailsRequested -= OnDetailsRequested;
            _barView.CloseRequested -= OnBarCloseRequested;
            _selection.Changed -= OnSelectionChanged;
            _clock.TimeAdvanced -= OnTimeAdvanced;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnDetailsRequested()
        {
            _selection.OpenFleetDetails();
        }

        private void OnDetailsCloseRequested()
        {
            _selection.CloseFleetDetails();
        }

        private void OnBarCloseRequested()
        {
            _selection.SelectFleet(EntityId.None);
        }

        private void OnSplitRequested(ContentId classId, int count)
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedFleetId.IsNone)
            {
                return;
            }

            var result = _fleetService.Split(run, _selection.SelectedFleetId, classId, count, out _);
            _status.PostCommand(result.Success, "Fleet split.", result.Error.Message);
        }

        private void OnMergeRequested(EntityId sourceFleetId)
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedFleetId.IsNone)
            {
                return;
            }

            var result = _fleetService.Merge(run, _selection.SelectedFleetId, sourceFleetId);
            _status.PostCommand(result.Success, "Fleet merged.", result.Error.Message);
        }

        private void OnSelectionChanged()
        {
            Apply();
        }

        private void OnTimeAdvanced(GameTime time, float deltaSeconds)
        {
            if (_countdown.ShouldRefresh(time, deltaSeconds))
            {
                Apply();
            }
        }

        private void Apply()
        {
            var model = _query.Describe(CurrentRun(), _selection);
            _countdown.SetTraveling(model.HasFleet && model.Route.IsTraveling, _clock);
            _barView.Apply(model);
            _detailsView.Apply(model);
        }

        private RunState CurrentRun()
        {
            return _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
