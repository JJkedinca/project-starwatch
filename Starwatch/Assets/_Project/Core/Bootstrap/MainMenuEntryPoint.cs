using System;
using Cysharp.Threading.Tasks;
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

        public MainMenuEntryPoint(MainMenuViewBridge viewBridge, RunStartService runStartService)
        {
            _viewBridge = viewBridge ?? throw new ArgumentNullException(nameof(viewBridge));
            _runStartService = runStartService ?? throw new ArgumentNullException(nameof(runStartService));
        }

        /// <summary>
        /// Subscribes to menu intents and binds the UI Toolkit document.
        /// </summary>
        public void Start()
        {
            _viewBridge.StartRunRequested += OnStartRunRequested;
            _viewBridge.QuitRequested += OnQuitRequested;
            _viewBridge.Bind();
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
        /// Disables Start Run while a transition is in flight.
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
