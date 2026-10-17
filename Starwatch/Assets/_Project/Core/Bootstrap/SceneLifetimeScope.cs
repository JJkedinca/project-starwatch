using UnityEngine;
using VContainer.Unity;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Base lifetime scope for scene containers. Automatically parents to the
    /// project scope so scene services can resolve run-wide dependencies.
    /// </summary>
    public abstract class SceneLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// Ensures a ProjectLifetimeScope parent reference exists before VContainer builds.
        /// </summary>
        protected override void Awake()
        {
            if (parentReference.Type == null && parentReference.Object == null)
            {
                parentReference = ParentReference.Create<ProjectLifetimeScope>();
            }

            if (FindAnyObjectByType<ProjectLifetimeScope>() == null)
            {
                Debug.LogError($"{GetType().Name} requires a ProjectLifetimeScope parent.");
            }

            base.Awake();
        }
    }
}
