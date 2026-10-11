using UnityEngine;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Entry MonoBehaviour for Bootstrap.unity. Ensures the DontDestroyOnLoad
    /// project systems object exists before any gameplay scene loads.
    /// </summary>
    public sealed class BootstrapLoader : MonoBehaviour
    {
        private const string kProjectSystemsName = "ProjectSystems";

        private void Awake()
        {
            EnsureProjectSystems();
        }

        /// <summary>
        /// Returns the existing ProjectSystems instance, or creates one if missing.
        /// </summary>
        public static ProjectSystems EnsureProjectSystems()
        {
            var existing = FindFirstObjectByType<ProjectSystems>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(kProjectSystemsName);
            DontDestroyOnLoad(go);
            return go.AddComponent<ProjectSystems>();
        }
    }
}
