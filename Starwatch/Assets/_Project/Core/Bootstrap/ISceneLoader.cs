using Cysharp.Threading.Tasks;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Abstraction over Unity scene loading so SceneFlow can be tested without
    /// depending on SceneManager directly.
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads a scene as the only active scene, unloading the previous one.
        /// </summary>
        UniTask LoadSingleAsync(SceneId sceneId);

        /// <summary>
        /// Loads a scene on top of currently loaded scenes.
        /// </summary>
        UniTask LoadAdditiveAsync(SceneId sceneId);

        /// <summary>
        /// Unloads a previously loaded scene if it is present.
        /// </summary>
        UniTask UnloadAsync(SceneId sceneId);

        /// <summary>
        /// Returns whether the requested scene is currently loaded.
        /// </summary>
        bool IsLoaded(SceneId sceneId);
    }
}
