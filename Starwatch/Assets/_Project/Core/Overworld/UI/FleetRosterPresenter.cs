using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Keeps the left fleet roster in sync on the map and in a star system.
    /// Transit ETAs sample the clock about once a second while any fleet is traveling.
    /// </summary>
    internal sealed class FleetRosterPresenter : IStartable, IDisposable
    {
        private const float kRefreshSeconds = 1f;

        private readonly IFleetRosterView _view;
        private readonly OverworldSelection _selection;
        private readonly RunManager _runManager;
        private readonly FleetListQuery _fleetListQuery;
        private readonly SimulationClock _clock;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private bool _transitActive;
        private double _nextRefresh;

        public FleetRosterPresenter(
            IFleetRosterView view,
            OverworldSelection selection,
            RunManager runManager,
            FleetListQuery fleetListQuery,
            SimulationClock clock,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _fleetListQuery = fleetListQuery ?? throw new ArgumentNullException(nameof(fleetListQuery));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.FleetSelected += OnFleetSelected;
            _selection.Changed += OnSelectionChanged;
            _clock.TimeAdvanced += OnTimeAdvanced;
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetCreatedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetMergedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<FleetSplitEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<RewindCompletedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _view.FleetSelected -= OnFleetSelected;
            _selection.Changed -= OnSelectionChanged;
            _clock.TimeAdvanced -= OnTimeAdvanced;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnFleetSelected(EntityId fleetId)
        {
            _selection.SelectFleet(fleetId);
        }

        private void OnSelectionChanged()
        {
            Apply();
        }

        private void OnTimeAdvanced(GameTime time, float deltaSeconds)
        {
            if (!_transitActive || deltaSeconds <= 0f || time.Seconds + 0.0001d < _nextRefresh)
            {
                return;
            }

            _nextRefresh = time.Seconds + kRefreshSeconds;
            Apply();
        }

        private void Apply()
        {
            if (!_runManager.HasActiveRun)
            {
                _transitActive = false;
                _view.Apply(null);
                return;
            }

            var run = _runManager.CurrentRun;
            var traveling = run.Timeline.Fleets.ActiveTravels.Count > 0;
            if (traveling && !_transitActive)
            {
                _nextRefresh = _clock.CurrentTime.Seconds + kRefreshSeconds;
            }

            _transitActive = traveling;
            _view.Apply(_fleetListQuery.All(run, _selection.SelectedFleetId));
        }
    }
}
