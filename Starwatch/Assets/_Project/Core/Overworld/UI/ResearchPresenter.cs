using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Research;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Research intents. Opens in the right column, replacing the target / settlement panel.
    /// A closed panel skips the science refresh. Opening redraws it.
    /// </summary>
    internal sealed class ResearchPresenter : IStartable, IDisposable
    {
        private readonly IResearchView _view;
        private readonly OverworldSelection _selection;
        private readonly OverworldStatus _status;
        private readonly RunManager _runManager;
        private readonly ResearchService _researchService;
        private readonly ResearchQuery _query;
        private readonly DomainEventBus _events;
        private readonly List<IDisposable> _subscriptions = new();
        private ContentId _shownActiveId;

        public ResearchPresenter(
            IResearchView view,
            OverworldSelection selection,
            OverworldStatus status,
            RunManager runManager,
            ResearchService researchService,
            ResearchQuery query,
            DomainEventBus events)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            _view.StartRequested += OnStartRequested;
            _view.SwitchConfirmed += OnSwitchConfirmed;
            _view.ToggleRequested += OnToggleRequested;
            _selection.Changed += OnSelectionChanged;
            _researchService.ScienceApplied += OnScienceApplied;
            _subscriptions.Add(_events.Subscribe<ResearchStartedEvent>(_ => Apply()));
            _subscriptions.Add(_events.Subscribe<ResearchCompletedEvent>(_ => Apply()));
            SyncOpenFromSelection();
            Apply();
        }

        public void Dispose()
        {
            _view.StartRequested -= OnStartRequested;
            _view.SwitchConfirmed -= OnSwitchConfirmed;
            _view.ToggleRequested -= OnToggleRequested;
            _selection.Changed -= OnSelectionChanged;
            _researchService.ScienceApplied -= OnScienceApplied;
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

            var result = _researchService.Start(run, researchId);
            _status.PostCommand(result.Success, "Research started.", result.Error.Message);
            Apply();
        }

        private void OnSwitchConfirmed(ContentId researchId)
        {
            var run = CurrentRun();
            if (run == null)
            {
                return;
            }

            var result = _researchService.Switch(run, researchId);
            _status.PostCommand(result.Success, "Research switched.", result.Error.Message);
            Apply();
        }

        private void OnToggleRequested()
        {
            if (_selection.RightColumnFocus == RightColumnFocus.Research)
            {
                _selection.CloseResearch();
                return;
            }

            _selection.OpenResearch();
        }

        private void OnSelectionChanged()
        {
            var wasOpen = _view.IsOpen;
            SyncOpenFromSelection();
            if (_view.IsOpen && !wasOpen)
            {
                Apply();
            }
        }

        private void OnScienceApplied()
        {
            if (!_view.IsOpen)
            {
                return;
            }

            Apply();
        }

        private void SyncOpenFromSelection()
        {
            var open = _selection.RightColumnFocus == RightColumnFocus.Research;
            _view.SetOpen(open);
            if (!open)
            {
                _view.DismissSwitch();
            }
        }

        private void Apply()
        {
            var model = _query.Describe(CurrentRun());
            var activeId = model.HasActive ? model.ActiveId : default;
            if (activeId != _shownActiveId)
            {
                _view.DismissSwitch();
            }

            _shownActiveId = activeId;
            _view.Apply(model);
        }

        private RunState CurrentRun()
        {
            return _runManager != null && _runManager.HasActiveRun ? _runManager.CurrentRun : null;
        }
    }
}
