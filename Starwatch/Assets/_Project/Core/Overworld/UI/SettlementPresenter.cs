using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
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
        private readonly SettlementService _settlement;
        private readonly BuildingService _buildings;
        private readonly SettlementQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public SettlementPresenter(
            ISettlementView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            SettlementService settlement,
            BuildingService buildings,
            SettlementQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _settlement = settlement ?? throw new ArgumentNullException(nameof(settlement));
            _buildings = buildings ?? throw new ArgumentNullException(nameof(buildings));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.FoundOutpostRequested += OnFoundOutpostRequested;
            _view.FoundColonyRequested += OnFoundColonyRequested;
            _view.AbandonRequested += OnAbandonRequested;
            _view.PlaceRequested += OnPlaceRequested;
            _view.UpgradeRequested += OnUpgradeRequested;
            _selection.Changed += OnSelectionChanged;
            _subscriptions.Add(_events.Subscribe<SettlementFoundedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<SettlementAbandonedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<BuildingChangedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<ResourceChangedEvent>(_ => OnSettlementChanged()));
            _subscriptions.Add(_events.Subscribe<MapKnowledgeChangedEvent>(OnMapKnowledgeChanged));
            Apply();
        }

        public void Dispose()
        {
            _view.FoundOutpostRequested -= OnFoundOutpostRequested;
            _view.FoundColonyRequested -= OnFoundColonyRequested;
            _view.AbandonRequested -= OnAbandonRequested;
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
            CommitPlanet(
                (run, planetId) => _settlement.FoundOutpost(run, ObserverId.Player, planetId),
                "Outpost founded.");
        }

        private void OnFoundColonyRequested()
        {
            CommitPlanet(
                (run, planetId) => _settlement.FoundColony(run, ObserverId.Player, planetId),
                "Colony founded.");
        }

        private void OnAbandonRequested()
        {
            CommitPlanet((run, planetId) => _settlement.Abandon(run, planetId), "Settlement abandoned.");
        }

        private void OnPlaceRequested(ContentId buildingId)
        {
            CommitPlanet((run, planetId) => _buildings.Place(run, planetId, buildingId), "Building placed.");
        }

        private void OnUpgradeRequested(ContentId buildingId)
        {
            CommitPlanet((run, planetId) => _buildings.Upgrade(run, planetId, buildingId), "Building upgraded.");
        }

        private void CommitPlanet(Func<RunState, ContentId, CommandResult> command, string success)
        {
            var run = CurrentRun();
            if (run == null || !_selection.InStarSystem || _selection.SelectedPlanetId.IsEmpty)
            {
                return;
            }

            var result = command(run, _selection.SelectedPlanetId);
            _status.Post(result.Success ? success : result.Error.Message);
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
