using VContainer;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for Overworld. Will host map, travel, economy, research, and
    /// invasion registrations as those systems come online.
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
