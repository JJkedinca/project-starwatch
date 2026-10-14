using EndlessHeros.Core.Content;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Persistence;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.Systems;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace EndlessHeros.Core.Bootstrap
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

            builder.RegisterInstance(_updateManager);
            builder.RegisterInstance(_objectDatabase);
            builder.Register<SimulationClock>(Lifetime.Singleton);
            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.Register<ISceneLoader, UnitySceneLoader>(Lifetime.Singleton);
            builder.Register<SceneFlow>(Lifetime.Singleton);
            builder.Register<TimelineFactory>(Lifetime.Singleton);
            builder.Register<EventJournal>(Lifetime.Singleton);
            builder.Register<UniverseLoader>(Lifetime.Singleton);
            builder.Register<RouteService>(Lifetime.Singleton);
            builder.Register<ArchiveService>(Lifetime.Singleton);
            builder.Register<VisibilityService>(Lifetime.Singleton);
            builder.Register<ExplorationService>(Lifetime.Singleton);
            builder.Register<SensorService>(Lifetime.Singleton);
            builder.Register<TravelService>(Lifetime.Singleton);
            builder.Register<TravelSystem>(Lifetime.Singleton);
            builder.Register<RunManager>(Lifetime.Singleton);
            builder.Register<IRunStateStore, InMemoryRunStateStore>(Lifetime.Singleton);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<GameEntryPoint>();
            builder.RegisterBuildCallback(container =>
            {
                var loop = container.Resolve<SimulationLoop>();
                // Deterministic order: travel resolution after clock advance (more systems append later).
                loop.Register(container.Resolve<TravelSystem>());
                _updateManager.Configure(loop);
            });
        }
    }
}
