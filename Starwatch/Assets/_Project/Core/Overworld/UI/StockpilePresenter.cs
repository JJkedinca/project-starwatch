using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Research;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Empire stockpile bar. Stock amounts refresh on the economy publish window;
    /// science ticks only refresh the research segment.
    /// </summary>
    internal sealed class StockpilePresenter : IStartable, IDisposable
    {
        private readonly IStockpileView _view;
        private readonly RunManager _runManager;
        private readonly StockpileQuery _query;
        private readonly ResearchService _researchService;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();

        public StockpilePresenter(
            IStockpileView view,
            RunManager runManager,
            StockpileQuery query,
            ResearchService researchService,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _researchService.ScienceApplied += OnScienceApplied;
            _subscriptions.Add(_events.Subscribe<ResourceChangedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<SettlementFoundedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<SettlementAbandonedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<BuildingChangedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchStartedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ShipQueuedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<RewindCompletedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _researchService.ScienceApplied -= OnScienceApplied;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnScienceApplied()
        {
            var run = CurrentRun();
            _view.ApplyResearchProgress(_query.Describe(run));
        }

        private void Apply()
        {
            _view.Apply(_query.Describe(CurrentRun()));
        }

        private RunState CurrentRun()
        {
            return _runManager != null && _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
