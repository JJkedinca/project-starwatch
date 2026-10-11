using System;
using Cysharp.Threading.Tasks;
using EndlessHeros.Core.Bootstrap;
using EndlessHeros.Core.Simulation;
using UnityEngine;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Main-menu command handler that creates a run and requests the overworld scene.
    /// </summary>
    public sealed class RunStartService
    {
        private readonly RunManager _runManager;
        private readonly SceneFlow _sceneFlow;
        private readonly SimulationClock _simulationClock;
        private bool _isStarting;

        public RunStartService(
            RunManager runManager,
            SceneFlow sceneFlow,
            SimulationClock simulationClock)
        {
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _sceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
            _simulationClock = simulationClock ?? throw new ArgumentNullException(nameof(simulationClock));
        }

        /// <summary>
        /// True while a start-run transition is already in progress.
        /// </summary>
        public bool IsStarting => _isStarting;

        /// <summary>
        /// Creates the active run from configuration and loads Overworld on success.
        /// Resets simulation time at run creation and resumes the clock after load.
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
                _simulationClock.ResetForRun();

                var seedValue = configuration.Seed ?? Environment.TickCount;
                var run = _runManager.CreateRun(configuration.ScenarioId, RunSeed.FromValue(seedValue));
                if (run == null)
                {
                    return false;
                }

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
