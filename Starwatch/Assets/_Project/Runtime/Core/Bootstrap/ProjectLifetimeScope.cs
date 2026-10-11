using EndlessHeros.Core.Content;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Persistence;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
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
        /// simulation clock after the container builds.
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
            builder.Register<ISceneLoader, UnitySceneLoader>(Lifetime.Singleton);
            builder.Register<SceneFlow>(Lifetime.Singleton);
            builder.Register<TimelineFactory>(Lifetime.Singleton);
            builder.Register<RunManager>(Lifetime.Singleton);
            builder.Register<ContentCatalog>(Lifetime.Singleton);
            builder.Register<EventJournal>(Lifetime.Singleton);
            builder.Register<IRunStateStore, InMemoryRunStateStore>(Lifetime.Singleton);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<GameEntryPoint>();
            builder.RegisterBuildCallback(container =>
            {
                var clock = container.Resolve<SimulationClock>();
                updateManager.Configure(clock);
            });
        }
    }
}
