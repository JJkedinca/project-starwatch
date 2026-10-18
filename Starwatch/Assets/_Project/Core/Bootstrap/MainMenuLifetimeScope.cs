using ProjectStarwatch.Core.Run;
using VContainer;
using VContainer.Unity;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for MainMenu. Registers run-start services and the menu presenter entry point.
    /// </summary>
    public sealed class MainMenuLifetimeScope : SceneLifetimeScope
    {
        /// <summary>
        /// Wires menu view bridge, run start service, and presenter entry point.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<RunStartService>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<MainMenuViewBridge>();
            builder.RegisterEntryPoint<MainMenuEntryPoint>();
        }
    }
}
