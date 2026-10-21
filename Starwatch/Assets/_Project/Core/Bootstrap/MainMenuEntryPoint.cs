using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Run;
using UnityEngine;
using VContainer.Unity;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Main-menu presenter entry point. Binds UI events to run-start and quit intents.
    /// </summary>
    internal sealed class MainMenuEntryPoint : IStartable
    {
        private readonly MainMenuViewBridge _viewBridge;
        private readonly RunStartService _runStartService;
        private readonly ObjectDatabaseSO _database;

        public MainMenuEntryPoint(
            MainMenuViewBridge viewBridge,
            RunStartService runStartService,
            ObjectDatabaseSO database)
        {
            _viewBridge = viewBridge ?? throw new ArgumentNullException(nameof(viewBridge));
            _runStartService = runStartService ?? throw new ArgumentNullException(nameof(runStartService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Subscribes to menu intents and binds the UI Toolkit document.
        /// </summary>
        public void Start()
        {
            _viewBridge.StartRunRequested += OnStartRunRequested;
            _viewBridge.QuitRequested += OnQuitRequested;
            _viewBridge.Bind();
            _viewBridge.SetScenarios(ListScenarioIds());
        }

        private List<string> ListScenarioIds()
        {
            List<string> ids = new();
            if (_database.Scenarios == null)
            {
                return ids;
            }

            foreach (var pair in _database.Scenarios)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                {
                    ids.Add(pair.Key);
                }
            }

            ids.Sort(StringComparer.Ordinal);
            return ids;
        }

        private void OnStartRunRequested()
        {
            if (_runStartService.IsStarting)
            {
                return;
            }

            StartRunAsync().Forget();
        }

        /// <summary>
        /// Creates a run from the menu fields and transitions into Overworld.
        /// Disables Launch while a transition is in flight.
        /// </summary>
        private async UniTaskVoid StartRunAsync()
        {
            _viewBridge.SetStartRunInteractable(false);

            try
            {
                var configuration = new RunStartConfiguration(
                    _viewBridge.ScenarioId,
                    _viewBridge.TryGetSeed(out var seed) ? seed : (int?)null);

                var started = await _runStartService.StartRunAsync(configuration);
                if (!started)
                {
                    Debug.LogError("MainMenuEntryPoint failed to start a run.");
                    _viewBridge.SetStartRunInteractable(true);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"MainMenuEntryPoint failed to start a run: {exception}");
                _viewBridge.SetStartRunInteractable(true);
            }
        }

        private void OnQuitRequested()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
