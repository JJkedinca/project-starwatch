using System;
using Cysharp.Threading.Tasks;
using ProjectStarwatch.Core.Bootstrap;
using ProjectStarwatch.Core.Simulation;
using UnityEngine;

namespace ProjectStarwatch.Core.Run
{
    /// <summary>
    /// Main-menu command handler that creates a run and requests the overworld scene.
    /// </summary>
    internal sealed class RunStartService
    {
        /// <summary>
        /// True while a start-run transition is already in progress.
        /// </summary>
        public bool IsStarting => _isStarting;

        private readonly RunManager _runManager;
        private readonly SceneFlow _sceneFlow;
        private readonly SimulationClock _simulationClock;
        private readonly SimulationLoop _simulationLoop;
        private bool _isStarting;

        public RunStartService(
            RunManager runManager,
            SceneFlow sceneFlow,
            SimulationClock simulationClock,
            SimulationLoop simulationLoop)
        {
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _sceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
            _simulationClock = simulationClock ?? throw new ArgumentNullException(nameof(simulationClock));
            _simulationLoop = simulationLoop ?? throw new ArgumentNullException(nameof(simulationLoop));
        }

        /// <summary>
        /// Creates the active run from configuration and loads Overworld on success.
        /// Resets simulation time only after CreateRun succeeds, then resumes after load.
        /// Rejects overlapping start requests.
        /// </summary>
        public async UniTask<bool> StartRunAsync(RunStartConfiguration configuration)
        {
            if (_isStarting)
            {
                Debug.LogWarning("RunStartService.StartRunAsync ignored overlapping start request.");
                return false;
            }

            if (configuration == null)
            {
                Debug.LogError("RunStartService.StartRunAsync requires a configuration.");
                return false;
            }

            _isStarting = true;
            try
            {
                var seedValue = configuration.Seed ?? Environment.TickCount;
                var run = _runManager.CreateRun(configuration.ScenarioId, RunSeed.FromValue(seedValue));
                if (run == null)
                {
                    return false;
                }

                _simulationClock.ResetForRun();
                _simulationLoop.ResetAccumulator();

                await _sceneFlow.LoadOverworldAsync();
                _simulationClock.SetPaused(false);
                return true;
            }
            finally
            {
                _isStarting = false;
            }
        }
    }
}
