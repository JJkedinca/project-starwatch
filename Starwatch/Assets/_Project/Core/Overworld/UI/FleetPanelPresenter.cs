using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Fleet;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Opens the fleet panel for the selected fleet and closes it when the selection clears.
    /// </summary>
    internal sealed class FleetPanelPresenter : IStartable, IDisposable
    {
        private readonly IFleetPanelView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly FleetService _fleets;
        private readonly FleetPanelQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public FleetPanelPresenter(
            IFleetPanelView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            FleetService fleets,
            FleetPanelQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _fleets = fleets ?? throw new ArgumentNullException(nameof(fleets));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.CloseRequested += OnCloseRequested;
            _view.SplitRequested += OnSplitRequested;
            _view.MergeRequested += OnMergeRequested;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetCreatedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetMergedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetSplitEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _view.CloseRequested -= OnCloseRequested;
            _view.SplitRequested -= OnSplitRequested;
            _view.MergeRequested -= OnMergeRequested;
            _selection.Changed -= OnSelectionChanged;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnCloseRequested()
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

            var result = _fleets.Split(run, _selection.SelectedFleetId, classId, count, out _);
            _status.Post(result.Success ? "Fleet split." : result.Error.Message);
        }

        private void OnMergeRequested(EntityId sourceFleetId)
        {
            var run = CurrentRun();
            if (run == null || _selection.SelectedFleetId.IsNone)
            {
                return;
            }

            var result = _fleets.Merge(run, _selection.SelectedFleetId, sourceFleetId);
            _status.Post(result.Success ? "Fleet merged." : result.Error.Message);
        }

        private void OnSelectionChanged()
        {
            Apply();
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
