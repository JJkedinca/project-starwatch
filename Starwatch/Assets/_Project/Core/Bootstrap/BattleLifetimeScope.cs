using VContainer;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for the additive Battle presentation. Owns battle-only
    /// simulation and view registrations, not run or engagement state.
    /// TODO: Replace empty Configure with battle simulation/view registrations in Phase 4.
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
