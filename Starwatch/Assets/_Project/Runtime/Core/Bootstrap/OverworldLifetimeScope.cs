using EndlessHeros.Overworld.UI;
using VContainer;
using VContainer.Unity;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for Overworld. Registers map presentation for Phase 1 Step 1.
    /// </summary>
    public sealed class OverworldLifetimeScope : SceneLifetimeScope
    {
        /// <summary>
        /// Wires the overworld view bridge and map presenter entry point.
        /// Project-scoped map services already live in ProjectLifetimeScope.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<OverworldViewBridge>();
            builder.RegisterEntryPoint<OverworldMapPresenter>();
        }
    }
}
