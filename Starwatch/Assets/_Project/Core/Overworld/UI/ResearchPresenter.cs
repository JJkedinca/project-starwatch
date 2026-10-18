using System;
using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Research;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Research intents. A closed panel skips the per-second refresh. An open panel refreshes only when the contents change.
    /// </summary>
    internal sealed class ResearchPresenter : IStartable, IDisposable
    {
        private readonly IResearchView _view;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly ResearchService _research;
        private readonly ResearchQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private ResearchPanelStamp _shown;
        private bool _hasShown;

        public ResearchPresenter(
            IResearchView view,
            OverworldStatus status,
            RunManager runManager,
            ResearchService research,
            ResearchQuery query,
            DomainEventBus events)
        {
            _view = view;
            _status = status;
            _runManager = runManager;
            _research = research;
            _query = query;
            _events = events;
        }

        public void Start()
        {
            _view.StartRequested += OnStartRequested;
            _view.SwitchConfirmed += OnSwitchConfirmed;
            _view.Opened += OnOpened;
            _research.ScienceApplied += OnScienceApplied;
            _subscriptions.Add(_events.Subscribe<ResearchStartedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            Apply();
        }

        public void Dispose()
        {
            _view.StartRequested -= OnStartRequested;
            _view.SwitchConfirmed -= OnSwitchConfirmed;
            _view.Opened -= OnOpened;
            _research.ScienceApplied -= OnScienceApplied;
            for (var i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i].Dispose();
            }

            _subscriptions.Clear();
        }

        private void OnStartRequested(ContentId researchId)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            if (run.Timeline.Research.HasActive && run.Timeline.Research.ActiveId != researchId)
            {
                _view.PromptSwitch(researchId);
                return;
            }

            var result = _research.Start(run, researchId);
            _status.Post(result.Success ? "Research started." : result.Error.Message);
            Apply();
        }

        private void OnSwitchConfirmed(ContentId researchId)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            var result = _research.Switch(run, researchId);
            _status.Post(result.Success ? "Research switched." : result.Error.Message);
            Apply();
        }

        private void OnOpened()
        {
            ApplyIfChanged();
        }

        private void OnScienceApplied()
        {
            if (!_view.IsOpen)
            {
                return;
            }

            ApplyIfChanged();
        }

        private void Apply()
        {
            var run = CurrentRun();
            _view.Apply(_query.Describe(run));
            _shown = _query.Stamp(run);
            _hasShown = true;
        }

        private void ApplyIfChanged()
        {
            var run = CurrentRun();
            var stamp = _query.Stamp(run);
            if (_hasShown && stamp.Equals(_shown))
            {
                return;
            }

            _view.Apply(_query.Describe(run));
            _shown = stamp;
            _hasShown = true;
        }

        private RunState CurrentRun()
        {
            return _runManager != null && _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
