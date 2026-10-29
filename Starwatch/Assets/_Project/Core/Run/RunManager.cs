using System;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Generation;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using UnityEngine;

namespace ProjectStarwatch.Core.Run
{
    /// <summary>
    /// Owns the active run and exposes read-only access to universe, timeline,
    /// archive, and rewind index. Feature services mutate through dedicated APIs.
    /// </summary>
    internal sealed class RunManager
    {
        public bool HasActiveRun => _currentRun != null;

        public RunState CurrentRun => _currentRun;

        /// <summary>
        /// Content/catalog lookup used by this manager. Generated planets register here on CreateRun.
        /// </summary>
        internal ObjectDatabaseSO ObjectDatabase => _objectDatabase;

        private readonly TimelineFactory _timelineFactory;
        private readonly UniverseLoader _universeLoader;
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly GalaxyGenerator _galaxyGenerator;
        private readonly EventJournal _eventJournal;
        private readonly DomainEventBus _events;
        private readonly SimulationLoop _simulationLoop;
        private RunState _currentRun;

        public RunManager(
            TimelineFactory timelineFactory,
            UniverseLoader universeLoader,
            ObjectDatabaseSO objectDatabase,
            GalaxyGenerator galaxyGenerator,
            EventJournal eventJournal,
            DomainEventBus events,
            SimulationLoop simulationLoop)
        {
            _timelineFactory = timelineFactory ?? throw new ArgumentNullException(nameof(timelineFactory));
            _universeLoader = universeLoader ?? throw new ArgumentNullException(nameof(universeLoader));
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _galaxyGenerator = galaxyGenerator ?? throw new ArgumentNullException(nameof(galaxyGenerator));
            _eventJournal = eventJournal ?? throw new ArgumentNullException(nameof(eventJournal));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _simulationLoop = simulationLoop ?? throw new ArgumentNullException(nameof(simulationLoop));
        }

        /// <summary>
        /// Creates a new run from configuration, clearing the project journal.
        /// Resolves an omitted seed to <see cref="Environment.TickCount"/>.
        /// </summary>
        public RunState CreateRun(
            RunStartConfiguration configuration,
            StartingEmpireDefinition startingEmpireOverride = null)
        {
            if (configuration == null)
            {
                Debug.LogError("RunManager.CreateRun requires a configuration.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(configuration.ScenarioId))
            {
                Debug.LogError("RunManager.CreateRun requires a scenario id.");
                return null;
            }

            var seed = RunSeed.FromValue(configuration.Seed ?? Environment.TickCount);
            if (!TryResolveScenario(
                    configuration.ScenarioId,
                    seed,
                    configuration.GalaxySizeId,
                    configuration.GalaxyShapeId,
                    out var definition,
                    out var generatedScenario))
            {
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
                // Generated scenarios are only registered after a successful load so a failed create
                // cannot replace the active run's planet/system content.
                if (generatedScenario)
                {
                    DestroyScenario(definition);
                }

                return null;
            }

            if (generatedScenario)
            {
                _objectDatabase.RegisterScenario(definition);
            }

            _eventJournal.Clear();
            var random = new DeterministicRandom(seed.Value);
            _currentRun = new RunState(seed, universe, timeline, archive, rewindIndex: 0, random, entityIds);
            _simulationLoop.NotifyRunChanged(_currentRun);
            return _currentRun;
        }

        /// <summary>
        /// Test/helper overload that builds a <see cref="RunStartConfiguration"/> from scenario and seed.
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

            return CreateRun(new RunStartConfiguration(scenarioId, seed.Value), startingEmpireOverride);
        }

        private bool TryResolveScenario(
            string scenarioId,
            RunSeed seed,
            GalaxySizeId galaxySizeId,
            GalaxyShapeId galaxyShapeId,
            out ScenarioDefinition definition,
            out bool generatedScenario)
        {
            definition = null;
            generatedScenario = false;
            if (scenarioId == GalaxyGenerator.ScenarioId)
            {
                if (!_galaxyGenerator.TryCreateScenario(
                        seed,
                        galaxySizeId,
                        galaxyShapeId,
                        _objectDatabase.SettlementCatalog,
                        out definition))
                {
                    Debug.LogError("RunManager.CreateRun failed to generate a galaxy scenario.");
                    return false;
                }

                generatedScenario = true;
                return true;
            }

            if (!_objectDatabase.TryGetScenario(scenarioId, out definition))
            {
                Debug.LogError($"RunManager.CreateRun could not find scenario '{scenarioId}'.");
                Debug.LogWarning($"Available scenarios: {string.Join(", ", _objectDatabase.Scenarios.Keys)}");
                return false;
            }

            return true;
        }

        private static void DestroyScenario(ScenarioDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(definition);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
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
            _events.Publish(new RewindCompletedEvent(_simulationLoop.Clock.CurrentTime, rewindIndex));
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
