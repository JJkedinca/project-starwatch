using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Star system intents: select a planet and return to the map.
    /// Owns the toggle between the map and the open system.
    /// </summary>
    internal sealed class StarSystemPresenter : IStartable, IDisposable
    {
        private readonly IStarSystemView _view;
        private readonly OverworldSelection _selection;
        private readonly RunManager _runManager;
        private readonly StarSystemQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public StarSystemPresenter(
            IStarSystemView view,
            OverworldSelection selection,
            RunManager runManager,
            StarSystemQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.BackRequested += OnBackRequested;
            _view.PlanetSelected += OnPlanetSelected;
            _view.FleetSelected += OnFleetSelected;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(_ => RefreshRoster()));
            _subscriptions.Add(_events.Subscribe<FleetCreatedEvent>(_ => RefreshRoster()));
            _subscriptions.Add(_events.Subscribe<FleetMergedEvent>(_ => RefreshRoster()));
            _subscriptions.Add(_events.Subscribe<FleetSplitEvent>(_ => RefreshRoster()));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(_ => RefreshRoster()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Refresh()));
            _subscriptions.Add(_events.Subscribe<RewindCompletedEvent>(OnRewind));
            Refresh();
        }

        public void Dispose()
        {
            _view.BackRequested -= OnBackRequested;
            _view.PlanetSelected -= OnPlanetSelected;
            _view.FleetSelected -= OnFleetSelected;
            _selection.Changed -= OnSelectionChanged;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnBackRequested()
        {
            if (!_selection.InStarSystem)
            {
                return;
            }

            _selection.BackToMap();
        }

        private void OnPlanetSelected(ContentId planetId)
        {
            _selection.SelectPlanet(planetId);
        }

        private void OnFleetSelected(EntityId fleetId)
        {
            _selection.SelectFleet(fleetId);
        }

        private void OnMapKnowledgeChanged(MapKnowledgeChangedEvent domainEvent)
        {
            if (domainEvent.ObserverId != ObserverId.Player
                || domainEvent.Kind != MapKnowledgeKind.PlanetScanned
                || !_selection.InStarSystem)
            {
                return;
            }

            Refresh();
        }

        private void OnSelectionChanged()
        {
            Refresh();
        }

        private void OnRewind(RewindCompletedEvent _)
        {
            if (!_selection.SelectedFleetId.IsNone)
            {
                _selection.SelectFleet(EntityId.None);
            }
        }

        private void RefreshRoster()
        {
            var selected = _selection.SelectedFleetId;
            var run = CurrentRun();
            if (!selected.IsNone && (run == null || !run.Timeline.Fleets.TryGetFleet(selected, out _)))
            {
                _selection.SelectFleet(EntityId.None);
                return;
            }

            Refresh();
        }

        private void Refresh()
        {
            _view.Apply(_query.Describe(CurrentRun(), _selection));
        }

        private RunState CurrentRun()
        {
            return _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
