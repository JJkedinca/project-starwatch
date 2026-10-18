using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ProjectStarwatch.Core.Bootstrap;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// Test double for ISceneLoader that records loads without touching Unity scenes.
    /// </summary>
    public sealed class FakeSceneLoader : ISceneLoader
    {
        private readonly HashSet<SceneId> _loaded = new();
        private readonly TaskCompletionSource<bool> _overworldGate = new();

        public List<SceneId> SingleLoads { get; } = new();

        public int LoadOverworldCount { get; private set; }

        public bool BlockOverworldLoad { get; set; }

        public async UniTask LoadSingleAsync(SceneId sceneId)
        {
            SingleLoads.Add(sceneId);
            if (sceneId == SceneId.Overworld)
            {
                LoadOverworldCount++;
                if (BlockOverworldLoad)
                {
                    await _overworldGate.Task;
                }
            }

            _loaded.Clear();
            _loaded.Add(sceneId);
            await UniTask.CompletedTask;
        }

        public async UniTask LoadAdditiveAsync(SceneId sceneId)
        {
            _loaded.Add(sceneId);
            await UniTask.CompletedTask;
        }

        public async UniTask UnloadAsync(SceneId sceneId)
        {
            _loaded.Remove(sceneId);
            await UniTask.CompletedTask;
        }

        public bool IsLoaded(SceneId sceneId)
        {
            return _loaded.Contains(sceneId);
        }

        public void ReleaseOverworldLoad()
        {
            _overworldGate.TrySetResult(true);
        }
    }
}
