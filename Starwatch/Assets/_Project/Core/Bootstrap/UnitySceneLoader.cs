using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Unity SceneManager implementation of ISceneLoader using UniTask awaits.
    /// </summary>
    internal sealed class UnitySceneLoader : ISceneLoader
    {
        /// <summary>
        /// Loads the scene in Single mode, replacing the active scene stack.
        /// </summary>
        public async UniTask LoadSingleAsync(SceneId sceneId)
        {
            var sceneName = ToSceneName(sceneId);
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start single load for scene '{sceneName}'.");
            }

            await operation.ToUniTask();
        }

        /// <summary>
        /// Loads the scene additively when it is not already present.
        /// </summary>
        public async UniTask LoadAdditiveAsync(SceneId sceneId)
        {
            var sceneName = ToSceneName(sceneId);
            if (IsLoaded(sceneId))
            {
                return;
            }

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start additive load for scene '{sceneName}'.");
            }

            await operation.ToUniTask();
        }

        /// <summary>
        /// Unloads the scene when Unity reports it as loaded.
        /// </summary>
        public async UniTask UnloadAsync(SceneId sceneId)
        {
            var sceneName = ToSceneName(sceneId);
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                return;
            }

            var operation = SceneManager.UnloadSceneAsync(scene);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start unload for scene '{sceneName}'.");
            }

            await operation.ToUniTask();
        }

        /// <summary>
        /// Checks whether Unity currently has the mapped scene loaded.
        /// </summary>
        public bool IsLoaded(SceneId sceneId)
        {
            var scene = SceneManager.GetSceneByName(ToSceneName(sceneId));
            return scene.IsValid() && scene.isLoaded;
        }

        /// <summary>
        /// Maps a SceneId to the Unity scene asset name registered in Build Settings.
        /// </summary>
        public static string ToSceneName(SceneId sceneId)
        {
            switch (sceneId)
            {
                case SceneId.Bootstrap:
                    return "Bootstrap";
                case SceneId.MainMenu:
                    return "MainMenu";
                case SceneId.Overworld:
                    return "Overworld";
                case SceneId.Battle:
                    return "Battle";
                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown scene id.");
            }
        }
    }
}
