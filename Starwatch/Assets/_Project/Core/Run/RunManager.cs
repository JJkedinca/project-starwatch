using System;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using UnityEngine;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Owns the active run and exposes read-only access to universe, timeline,
    /// archive, and rewind index. Feature services mutate through dedicated APIs.
    /// </summary>
    internal sealed class RunManager
    {
        public bool HasActiveRun => _currentRun != null;

        public RunState CurrentRun => _currentRun;

        private readonly TimelineFactory _timelineFactory;
        private readonly UniverseLoader _universeLoader;
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly EventJournal _eventJournal;
        private readonly SimulationLoop _simulationLoop;
        private RunState _currentRun;

        public RunManager(
            TimelineFactory timelineFactory,
            UniverseLoader universeLoader,
            ObjectDatabaseSO objectDatabase,
            EventJournal eventJournal,
            SimulationLoop simulationLoop)
        {
            _timelineFactory = timelineFactory ?? throw new ArgumentNullException(nameof(timelineFactory));
            _universeLoader = universeLoader ?? throw new ArgumentNullException(nameof(universeLoader));
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _eventJournal = eventJournal ?? throw new ArgumentNullException(nameof(eventJournal));
            _simulationLoop = simulationLoop ?? throw new ArgumentNullException(nameof(simulationLoop));
        }

        /// <summary>
        /// Creates a new run from the object database scenario, clearing the project journal.
        /// </summary>
        public RunState CreateRun(
            string scenarioId,
            RunSeed seed,
            StartingEmpireDefinition startingEmpireOverride = null)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                Debug.LogError("RunManager.CreateRun requires a scenario id.");
                return null;
            }

            if (!_objectDatabase.TryGetScenario(scenarioId, out var definition))
            {
                Debug.LogError($"RunManager.CreateRun could not find scenario '{scenarioId}'.");
                return null;
            }

            var entityIds = new EntityIdFactory();
            if (!_universeLoader.TryLoad(
                    definition,
                    entityIds,
                    out var universe,
                    out var archive,
                    out var timeline,
                    startingEmpireOverride))
            {
                return null;
            }

            _eventJournal.Clear();
            var random = new DeterministicRandom(seed.Value);
            _currentRun = new RunState(seed, universe, timeline, archive, rewindIndex: 0, random, entityIds);
            _simulationLoop.NotifyRunChanged(_currentRun);
            return _currentRun;
        }

        /// <summary>
        /// Replaces the current timeline using the factory without mutating universe/archive.
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

            var timeline = _timelineFactory.Create(_currentRun.Universe, _currentRun.Archive, _currentRun.EntityIds);
            if (!_currentRun.ReplaceTimeline(timeline, rewindIndex))
            {
                Debug.LogError("RunManager.RecreateTimeline failed to replace timeline state.");
                return null;
            }

            _simulationLoop.NotifyTimelineReset();
            return timeline;
        }

        /// <summary>
        /// Clears the active run and its journal without disposing project-scoped services.
        /// </summary>
        public void ClearRun()
        {
            _currentRun = null;
            _eventJournal.Clear();
            _simulationLoop.NotifyRunChanged(null);
        }
    }
}
