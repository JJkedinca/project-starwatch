using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Owns gameplay scene transitions so feature services never call
    /// SceneManager directly. Battle loads additively while overworld stays active.
    /// </summary>
    internal sealed class SceneFlow
    {
        public SceneId ActiveGameplayScene { get; private set; } = SceneId.Bootstrap;

        public bool IsBattlePresentationLoaded => _sceneLoader.IsLoaded(SceneId.Battle);

        private readonly ISceneLoader _sceneLoader;

        public SceneFlow(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        /// <summary>
        /// Replaces the current scene with MainMenu and clears any battle presentation.
        /// </summary>
        public async UniTask LoadMainMenuAsync()
        {
            await UnloadBattlePresentationAsync();
            await _sceneLoader.LoadSingleAsync(SceneId.MainMenu);
            ActiveGameplayScene = SceneId.MainMenu;
        }

        /// <summary>
        /// Replaces the current scene with Overworld and clears any battle presentation.
        /// </summary>
        public async UniTask LoadOverworldAsync()
        {
            await UnloadBattlePresentationAsync();
            await _sceneLoader.LoadSingleAsync(SceneId.Overworld);
            ActiveGameplayScene = SceneId.Overworld;
        }

        /// <summary>
        /// Loads Battle additively on top of Overworld for presentation only.
        /// </summary>
        public async UniTask LoadBattlePresentationAsync()
        {
            if (ActiveGameplayScene != SceneId.Overworld)
            {
                Debug.LogError("Battle presentation requires the overworld scene to remain loaded.");
                return;
            }

            await _sceneLoader.LoadAdditiveAsync(SceneId.Battle);
        }

        /// <summary>
        /// Unloads the additive battle presentation without recreating the run.
        /// </summary>
        public async UniTask UnloadBattlePresentationAsync()
        {
            if (_sceneLoader.IsLoaded(SceneId.Battle))
            {
                await _sceneLoader.UnloadAsync(SceneId.Battle);
            }
        }
    }
}
