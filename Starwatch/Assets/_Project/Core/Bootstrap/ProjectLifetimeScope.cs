using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Persistence;
using ProjectStarwatch.Core.Presentation;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Fleet;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Overworld.Systems;
using ProjectStarwatch.Research;
using ProjectStarwatch.Settlement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Root VContainer scope for services that must survive scene transitions,
    /// including the simulation clock, run manager, and scene flow.
    /// Authored on the Bootstrap scene so content references are assigned, not created.
    /// </summary>
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private UpdateManager _updateManager;

        [SerializeField]
        private ObjectDatabaseSO _objectDatabase;

        [SerializeField]
        private StarwatchPresentationDatabaseSO _presentationDatabase;

        /// <summary>
        /// Keeps this object alive when Bootstrap unloads into Main Menu.
        /// </summary>
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        /// <summary>
        /// Registers project-wide singletons and wires the update manager to the
        /// fixed-rate simulation loop after the container builds.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            if (_updateManager == null)
            {
                Debug.LogError("ProjectLifetimeScope requires an UpdateManager on the same GameObject.");
                return;
            }

            if (_objectDatabase == null)
            {
                Debug.LogError("ProjectLifetimeScope requires an assigned ObjectDatabaseSO.");
                return;
            }

            if (_presentationDatabase == null)
            {
                Debug.LogError("ProjectLifetimeScope requires an assigned StarwatchPresentationDatabaseSO.");
                return;
            }

            builder.RegisterInstance(_updateManager);
            builder.RegisterInstance(_objectDatabase);
            builder.RegisterInstance(_presentationDatabase);
            builder.Register<SimulationClock>(Lifetime.Singleton);
            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.Register<ISceneLoader, UnitySceneLoader>(Lifetime.Singleton);
            builder.Register<SceneFlow>(Lifetime.Singleton);
            builder.Register<TimelineFactory>(Lifetime.Singleton);
            builder.Register<EventJournal>(Lifetime.Singleton);
            builder.Register<DomainEventBus>(Lifetime.Singleton);
            builder.Register<UniverseLoader>(Lifetime.Singleton);
            builder.Register<RouteService>(Lifetime.Singleton);
            builder.Register<ArchiveService>(Lifetime.Singleton);
            builder.Register<VisibilityService>(Lifetime.Singleton);
            builder.Register<ExplorationService>(Lifetime.Singleton);
            builder.Register<SensorService>(Lifetime.Singleton);
            builder.Register<TravelService>(Lifetime.Singleton);
            builder.Register<TravelSystem>(Lifetime.Singleton);
            builder.Register<EffectRegistry>(Lifetime.Singleton);
            builder.Register<IEffectQuery>(resolver => resolver.Resolve<EffectRegistry>(), Lifetime.Singleton);
            builder.Register<ProductionService>(Lifetime.Singleton);
            builder.Register<EconomyService>(Lifetime.Singleton);
            builder.Register<EconomyTickSystem>(Lifetime.Singleton);
            builder.Register<ResearchService>(Lifetime.Singleton);
            builder.Register<ResearchTickSystem>(Lifetime.Singleton);
            builder.Register<SettlementService>(Lifetime.Singleton);
            builder.Register<BuildingService>(Lifetime.Singleton);
            builder.Register<FleetService>(Lifetime.Singleton);
            builder.Register<ShipProductionService>(Lifetime.Singleton);
            builder.Register<ShipProductionSystem>(Lifetime.Singleton);
            builder.Register<RunManager>(Lifetime.Singleton);
            builder.Register<IRunStateStore, InMemoryRunStateStore>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameEntryPoint>();
            builder.RegisterBuildCallback(container =>
            {
                var loop = container.Resolve<SimulationLoop>();
                var economyTick = container.Resolve<EconomyTickSystem>();
                // Travel, then economy, then research, then ship production.
                loop.Register(container.Resolve<TravelSystem>());
                loop.Register(economyTick);
                loop.Register(container.Resolve<ResearchTickSystem>());
                loop.Register(container.Resolve<ShipProductionSystem>());
                _updateManager.Configure(loop);
            });
        }
    }
}
