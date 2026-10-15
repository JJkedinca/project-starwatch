using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Project-scope startup hook. After the root container is ready, loads the
    /// main menu as the first player-facing scene.
    /// </summary>
    internal sealed class GameEntryPoint : IStartable
    {
        private readonly SceneFlow _sceneFlow;

        public GameEntryPoint(SceneFlow sceneFlow)
        {
            _sceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
        }

        /// <summary>
        /// Begins the async transition from Bootstrap into MainMenu.
        /// </summary>
        public void Start()
        {
            LoadMainMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMainMenuAsync()
        {
            try
            {
                await _sceneFlow.LoadMainMenuAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"GameEntryPoint failed to load MainMenu: {exception}");
            }
        }
    }
}
