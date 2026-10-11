using System;
using UnityEngine;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Owns the active run and exposes read-only access to scenario, timeline,
    /// archive, and rewind index. Feature services mutate through dedicated APIs.
    /// </summary>
    public sealed class RunManager
    {
        private readonly TimelineFactory _timelineFactory;
        private RunState _currentRun;

        public RunManager(TimelineFactory timelineFactory)
        {
            _timelineFactory = timelineFactory ?? throw new ArgumentNullException(nameof(timelineFactory));
        }

        public bool HasActiveRun => _currentRun != null;

        public RunState CurrentRun => _currentRun;

        /// <summary>
        /// Creates a new run with fresh scenario, timeline, and archive state.
        /// </summary>
        public RunState CreateRun(string scenarioId, RunSeed seed)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                Debug.LogError("RunManager.CreateRun requires a scenario id.");
                return null;
            }

            var scenario = new ScenarioState(scenarioId);
            var archive = new RunArchive();
            var timeline = _timelineFactory.Create(scenario, archive, rewindIndex: 0);
            _currentRun = new RunState(seed, scenario, timeline, archive, rewindIndex: 0);
            return _currentRun;
        }

        /// <summary>
        /// Clears the active run without disposing project-scoped services.
        /// </summary>
        public void ClearRun()
        {
            _currentRun = null;
        }
    }
}
