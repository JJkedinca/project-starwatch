using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Settlement;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Settlement intents. Rebuilds the panel for the open system.
    /// </summary>
    internal sealed class SettlementPresenter : IStartable, IDisposable
    {
        private readonly ISettlementView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly SettlementService _settlementService;
        private readonly BuildingService _buildingService;
        private readonly SensorService _sensorService;
        private readonly ObjectDatabaseSO _database;
        private readonly SettlementQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public SettlementPresenter(
            ISettlementView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            SettlementService settlementService,
            BuildingService buildingService,
            SensorService sensorService,
            ObjectDatabaseSO database,
            SettlementQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _settlementService = settlementService ?? throw new ArgumentNullException(nameof(settlementService));
            _buildingService = buildingService ?? throw new ArgumentNullException(nameof(buildingService));
            _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.FoundOutpostRequested += OnFoundOutpostRequested;
            _view.FoundColonyRequested += OnFoundColonyRequested;
            _view.AbandonRequested += OnAbandonRequested;
            _view.ScanRequested += OnScanRequested;
            _view.PlaceRequested += OnPlaceRequested;
            _view.UpgradeRequested += OnUpgradeRequested;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<SettlementFoundedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<SettlementAbandonedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<BuildingChangedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<ResourceChangedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            Apply();
        }

        public void Dispose()
        {
            _view.FoundOutpostRequested -= OnFoundOutpostRequested;
            _view.FoundColonyRequested -= OnFoundColonyRequested;
            _view.AbandonRequested -= OnAbandonRequested;
            _view.ScanRequested -= OnScanRequested;
            _view.PlaceRequested -= OnPlaceRequested;
            _view.UpgradeRequested -= OnUpgradeRequested;
            _selection.Changed -= OnSelectionChanged;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnFoundOutpostRequested()
        {
            // Success toast comes from OverworldToastPresenter (SettlementFoundedEvent).
            CommitPlanet(
                (run, planetId) => _settlementService.FoundOutpost(run, ObserverId.Player, planetId),
                successMessage: null);
        }

        private void OnFoundColonyRequested()
        {
            CommitPlanet(
                (run, planetId) => _settlementService.FoundColony(run, ObserverId.Player, planetId),
                successMessage: null);
        }

        private void OnAbandonRequested()
        {
            CommitPlanet((run, planetId) => _settlementService.Abandon(run, planetId), "Settlement abandoned.");
        }

        private void OnScanRequested()
        {
            var run = CurrentRun();
            var planetId = _selection.SelectedPlanetId;
            if (run == null || !_selection.InStarSystem || planetId.IsEmpty)
            {
                return;
            }

            var result = _sensorService.ScanPlanet(run, ObserverId.Player, planetId);
            if (!result.Success)
            {
                _status.Post(result.Error.Message, StatusKind.Error);
                return;
            }

            _status.Post(
                "Scanned " + OverworldNames.Planet(_database, planetId) + ".",
                StatusKind.Result);
        }

        private void OnPlaceRequested(ContentId buildingId)
        {
            CommitPlanet((run, planetId) => _buildingService.Place(run, planetId, buildingId), "Building placed.");
        }

        private void OnUpgradeRequested(ContentId buildingId)
        {
            CommitPlanet((run, planetId) => _buildingService.Upgrade(run, planetId, buildingId), "Building upgraded.");
        }

        private void CommitPlanet(Func<RunState, ContentId, CommandResult> command, string successMessage)
        {
            var run = CurrentRun();
            if (run == null || !_selection.InStarSystem || _selection.SelectedPlanetId.IsEmpty)
            {
                return;
            }

            var result = command(run, _selection.SelectedPlanetId);
            if (!result.Success)
            {
                _status.Post(result.Error.Message, StatusKind.Error);
                return;
            }

            if (!string.IsNullOrEmpty(successMessage))
            {
                _status.Post(successMessage, StatusKind.Result);
            }
        }

        private void OnMapKnowledgeChanged(MapKnowledgeChangedEvent domainEvent)
        {
            if (domainEvent.ObserverId != ObserverId.Player
                || domainEvent.Kind != MapKnowledgeKind.PlanetScanned
                || !_selection.InStarSystem)
            {
                return;
            }

            Apply();
        }

        private void OnSettlementChanged()
        {
            if (!_selection.InStarSystem)
            {
                return;
            }

            Apply();
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
