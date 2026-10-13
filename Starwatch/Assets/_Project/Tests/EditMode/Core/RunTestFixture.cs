using EndlessHeros.Core.Content;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.Systems;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// Shared construction helpers for EditMode run/overworld tests.
    /// </summary>
    public static class RunTestFixture
    {
        public static ContentCatalog CreateCatalog()
        {
            return new ContentCatalog();
        }

        public static RunManager CreateRunManager(ContentCatalog catalog = null, EventJournal journal = null)
        {
            catalog = catalog ?? CreateCatalog();
            journal = journal ?? new EventJournal();
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            return new RunManager(timelineFactory, loader, catalog, journal);
        }

        public static (
            RunManager Manager,
            ExplorationService Exploration,
            SensorService Sensor,
            TravelService Travel,
            VisibilityService Visibility,
            TravelSystem TravelSystem,
            SimulationLoop Loop,
            SimulationClock Clock,
            EventJournal Journal) CreateOverworldStack()
        {
            var catalog = CreateCatalog();
            var journal = new EventJournal();
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            var manager = new RunManager(timelineFactory, loader, catalog, journal);
            var route = new RouteService();
            var archive = new ArchiveService();
            var exploration = new ExplorationService(archive, route, journal, clock);
            var sensor = new SensorService(archive, catalog, journal, clock);
            var travel = new TravelService(route, clock);
            var visibility = new VisibilityService();
            var travelSystem = new TravelSystem(manager, journal);
            loop.Register(travelSystem);
            return (manager, exploration, sensor, travel, visibility, travelSystem, loop, clock, journal);
        }
    }
}
