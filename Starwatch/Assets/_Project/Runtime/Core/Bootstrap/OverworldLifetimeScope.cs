using VContainer;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for Overworld. Will host map, travel, economy, research, and
    /// invasion registrations as those systems come online.
    /// TODO: Replace empty Configure with overworld presenters/views when the UI pass lands;
    /// project-scoped map services already live in ProjectLifetimeScope.
    /// </summary>
    public sealed class OverworldLifetimeScope : SceneLifetimeScope
    {
        /// <summary>
        /// Placeholder configuration until overworld feature services are added.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
