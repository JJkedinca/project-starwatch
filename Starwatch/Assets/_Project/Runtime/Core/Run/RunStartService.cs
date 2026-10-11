using System;
using Cysharp.Threading.Tasks;
using EndlessHeros.Core.Bootstrap;
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

        public RunStartService(RunManager runManager, SceneFlow sceneFlow)
        {
            _runManager = runManager ?? throw new ArgumentNullException(nameof(runManager));
            _sceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
        }

        /// <summary>
        /// Creates the active run from configuration and loads Overworld on success.
        /// </summary>
        public async UniTask<bool> StartRunAsync(RunStartConfiguration configuration)
        {
            if (configuration == null)
            {
                Debug.LogError("RunStartService.StartRunAsync requires a configuration.");
                return false;
            }

            var seedValue = configuration.Seed ?? Environment.TickCount;
            var run = _runManager.CreateRun(configuration.ScenarioId, RunSeed.FromValue(seedValue));
            if (run == null)
            {
                return false;
            }

            await _sceneFlow.LoadOverworldAsync();
            return true;
        }
    }
}
