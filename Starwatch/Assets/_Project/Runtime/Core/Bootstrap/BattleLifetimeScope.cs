using VContainer;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for the additive Battle presentation. Owns battle-only
    /// simulation and view registrations, not run or engagement state.
    /// </summary>
    public sealed class BattleLifetimeScope : SceneLifetimeScope
    {
        /// <summary>
        /// Placeholder configuration until battle presentation services are added.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
        }
    }
}
