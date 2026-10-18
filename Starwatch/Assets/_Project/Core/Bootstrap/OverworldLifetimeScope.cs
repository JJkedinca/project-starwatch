using ProjectStarwatch.Overworld.UI;
using VContainer;
using VContainer.Unity;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Scene scope for Overworld. Registers the shared selection and the group presenters.
    /// Project-scoped services already live in ProjectLifetimeScope.
    /// </summary>
    public sealed class OverworldLifetimeScope : SceneLifetimeScope
    {
        /// <summary>
        /// Wires the screen, the shared selection and status, and the group presenters.
        /// The binder is the only entry point, so it binds the screen before those presenters start.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<OverworldScreenView>();
            builder.Register<OverworldSelection>(Lifetime.Scoped);
            builder.Register<OverworldStatus>(Lifetime.Scoped);
            builder.Register<StarMapQuery>(Lifetime.Scoped);
            builder.Register<StarSystemQuery>(Lifetime.Scoped);
            builder.Register<SettlementQuery>(Lifetime.Scoped);
            builder.Register<StockpileQuery>(Lifetime.Scoped);
            builder.Register<ResearchQuery>(Lifetime.Scoped);
            builder.Register<IStarMapView>(
                resolver => resolver.Resolve<OverworldScreenView>().StarMap,
                Lifetime.Scoped);
            builder.Register<IStarSystemView>(
                resolver => resolver.Resolve<OverworldScreenView>().StarSystem,
                Lifetime.Scoped);
            builder.Register<ISettlementView>(
                resolver => resolver.Resolve<OverworldScreenView>().Settlement,
                Lifetime.Scoped);
            builder.Register<IStockpileView>(
                resolver => resolver.Resolve<OverworldScreenView>().Stockpile,
                Lifetime.Scoped);
            builder.Register<IResearchView>(
                resolver => resolver.Resolve<OverworldScreenView>().Research,
                Lifetime.Scoped);
            builder.Register<StarMapPresenter>(Lifetime.Scoped);
            builder.Register<StarSystemPresenter>(Lifetime.Scoped);
            builder.Register<SettlementPresenter>(Lifetime.Scoped);
            builder.Register<StockpilePresenter>(Lifetime.Scoped);
            builder.Register<ResearchPresenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<OverworldScreenBinder>(Lifetime.Scoped);
        }
    }
}
