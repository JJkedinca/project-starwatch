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
    /// </summary>
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// Registers project-wide singletons and wires the update manager to the
        /// fixed-rate simulation loop after the container builds.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            var projectSystems = GetComponent<ProjectSystems>();
            var updateManager = projectSystems != null
                ? projectSystems.UpdateManager
                : GetComponent<UpdateManager>();

            if (projectSystems == null)
            {
                Debug.LogError("ProjectLifetimeScope requires a ProjectSystems component.");
                return;
            }

            if (updateManager == null)
            {
                Debug.LogError("ProjectLifetimeScope requires an UpdateManager on ProjectSystems.");
                return;
            }

            builder.RegisterInstance(projectSystems);
            builder.RegisterInstance(updateManager);
            builder.Register<SimulationClock>(Lifetime.Singleton);
            builder.Register<SimulationLoop>(Lifetime.Singleton);
            builder.Register<ISceneLoader, UnitySceneLoader>(Lifetime.Singleton);
            builder.Register<SceneFlow>(Lifetime.Singleton);
            builder.Register<TimelineFactory>(Lifetime.Singleton);
            builder.Register<ContentCatalog>(Lifetime.Singleton);
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
                updateManager.Configure(loop);
            });
        }
    }
}
