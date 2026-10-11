using EndlessHeros.Core.Simulation;
using UnityEngine;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// DontDestroyOnLoad host for project-wide systems such as the update
    /// manager and the root VContainer lifetime scope.
    /// </summary>
    public sealed class ProjectSystems : MonoBehaviour
    {
        [SerializeField]
        private ProjectLifetimeScope _projectLifetimeScope;

        [SerializeField]
        private UpdateManager _updateManager;

        public ProjectLifetimeScope ProjectLifetimeScope => _projectLifetimeScope;

        public UpdateManager UpdateManager => _updateManager;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureComponents();
        }

        /// <summary>
        /// Lazily creates required project components when they are not already
        /// serialized on this object.
        /// </summary>
        public void EnsureComponents()
        {
            if (_updateManager == null)
            {
                _updateManager = GetComponent<UpdateManager>();
                if (_updateManager == null)
                {
                    _updateManager = gameObject.AddComponent<UpdateManager>();
                }
            }

            if (_projectLifetimeScope == null)
            {
                _projectLifetimeScope = GetComponent<ProjectLifetimeScope>();
                if (_projectLifetimeScope == null)
                {
                    _projectLifetimeScope = gameObject.AddComponent<ProjectLifetimeScope>();
                }
            }
        }
    }
}
