using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Posts event toasts from domain events so feature presenters stay free of that noise.
    /// </summary>
    internal sealed class OverworldToastPresenter : IStartable, IDisposable
    {
        private readonly OverworldStatus _status;
        private readonly DomainEventBus _events;
        private readonly ObjectDatabaseSO _database;
        private readonly List<IDisposable> _subscriptions = new();

        public OverworldToastPresenter(
            OverworldStatus status,
            DomainEventBus events,
            ObjectDatabaseSO database)
        {
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public void Start()
        {
            _subscriptions.Add(_events.Subscribe<FleetTravelEvent>(OnFleetTravel));
            _subscriptions.Add(_events.Subscribe<ShipConstructionCompletedEvent>(OnShipCompleted));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(OnResearchCompleted));
            _subscriptions.Add(_events.Subscribe<SettlementFoundedEvent>(OnSettlementFounded));
        }

        public void Dispose()
        {
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnFleetTravel(FleetTravelEvent domainEvent)
        {
            if (domainEvent.Kind != FleetTravelKind.Arrived)
            {
                return;
            }

            _status.Post(
                "Fleet arrived at " + OverworldNames.StarSystem(_database, domainEvent.ToNodeId) + ".",
                StatusKind.Event);
        }

        private void OnShipCompleted(ShipConstructionCompletedEvent domainEvent)
        {
            _status.Post(
                "Ship completed: " + OverworldNames.ShipClass(_database, domainEvent.ClassId) + ".",
                StatusKind.Event);
        }

        private void OnResearchCompleted(ResearchCompletedEvent domainEvent)
        {
            _status.Post(
                "Research completed: " + OverworldNames.Research(_database, domainEvent.ResearchId) + ".",
                StatusKind.Event);
        }

        private void OnSettlementFounded(SettlementFoundedEvent domainEvent)
        {
            var kind = domainEvent.Kind == SettlementKind.Colony ? "Colony" : "Outpost";
            _status.Post(
                kind + " founded on " + OverworldNames.Planet(_database, domainEvent.PlanetId) + ".",
                StatusKind.Event);
        }
    }
}
