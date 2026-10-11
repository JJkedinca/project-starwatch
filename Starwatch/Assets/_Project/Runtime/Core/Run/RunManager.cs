using System;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;
using UnityEngine;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Owns the active run and exposes read-only access to scenario, timeline,
    /// archive, and rewind index. Feature services mutate through dedicated APIs.
    /// </summary>
    public sealed class RunManager
    {
        public bool HasActiveRun => _currentRun != null;

        public RunState CurrentRun => _currentRun;

        private readonly TimelineFactory _timelineFactory;
        private readonly EventJournal _eventJournal;
        private RunState _currentRun;

        public RunManager(TimelineFactory timelineFactory, EventJournal eventJournal)
        {
            _timelineFactory = timelineFactory ?? throw new ArgumentNullException(nameof(timelineFactory));
            _eventJournal = eventJournal ?? throw new ArgumentNullException(nameof(eventJournal));
        }

        /// <summary>
        /// Creates a new run with fresh scenario, timeline, archive, and seeded random.
        /// Clears the project journal so records belong only to the active run.
        /// </summary>
        public RunState CreateRun(
            string scenarioId,
            RunSeed seed,
            StartingEmpireDefinition startingEmpire = null)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                Debug.LogError("RunManager.CreateRun requires a scenario id.");
                return null;
            }

            _eventJournal.Clear();

            var empire = startingEmpire ?? StartingEmpireDefinition.CreateDefault();
            var scenario = new ScenarioState(scenarioId, empire);
            var archive = new RunArchive();
            var entityIds = new EntityIdFactory();
            var timeline = _timelineFactory.Create(scenario, archive, entityIds);
            var random = new DeterministicRandom(seed.Value);
            _currentRun = new RunState(seed, scenario, timeline, archive, rewindIndex: 0, random, entityIds);
            return _currentRun;
        }

        /// <summary>
        /// Replaces the current timeline using the factory without mutating scenario/archive.
        /// Used by tests now and rewind later.
        /// </summary>
        public TimelineState RecreateTimeline(int rewindIndex)
        {
            if (_currentRun == null)
            {
                Debug.LogError("RunManager.RecreateTimeline requires an active run.");
                return null;
            }

            if (rewindIndex < 0)
            {
                Debug.LogError("RunManager.RecreateTimeline requires a non-negative rewind index.");
                return null;
            }

            var timeline = _timelineFactory.Create(
                _currentRun.Scenario,
                _currentRun.Archive,
                _currentRun.EntityIds);
            if (!_currentRun.ReplaceTimeline(timeline, rewindIndex))
            {
                Debug.LogError("RunManager.RecreateTimeline failed to replace timeline state.");
                return null;
            }

            return timeline;
        }

        /// <summary>
        /// Clears the active run and its journal without disposing project-scoped services.
        /// </summary>
        public void ClearRun()
        {
            _currentRun = null;
            _eventJournal.Clear();
        }
    }
}
