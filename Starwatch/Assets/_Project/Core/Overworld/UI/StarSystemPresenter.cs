using System;
using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld.Services;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Star system intents: select a planet, scan it, and return to the map.
    /// Owns the toggle between the map and the open system.
    /// </summary>
    internal sealed class StarSystemPresenter : IStartable, IDisposable
    {
        private readonly IStarSystemView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly SensorService _sensor;
        private readonly StarSystemQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public StarSystemPresenter(
            IStarSystemView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            SensorService sensor,
            StarSystemQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.BackRequested += OnBackRequested;
            _view.PlanetSelected += OnPlanetSelected;
            _view.ScanRequested += OnScanRequested;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            Refresh();
        }

        public void Dispose()
        {
            _view.BackRequested -= OnBackRequested;
            _view.PlanetSelected -= OnPlanetSelected;
            _view.ScanRequested -= OnScanRequested;
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
            if (_runManager.HasActiveRun)
            {
                _status.Post(OverworldStatus.kMapHint);
            }
        }

        private void OnPlanetSelected(ContentId planetId)
        {
            _selection.SelectPlanet(planetId);
        }

        private void OnScanRequested(ContentId planetId)
        {
            var run = CurrentRun();
            if (run == null || planetId.IsEmpty)
            {
                return;
            }

            var result = _sensor.ScanPlanet(run, ObserverId.Player, planetId);
            if (!result.Success)
            {
                _status.Post(result.Error.Message);
                return;
            }

            _selection.SelectPlanet(planetId);
            _status.Post("Scanned " + planetId.Value + ".");
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
