using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.Systems;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// Shared construction helpers for EditMode run/overworld tests.
    /// </summary>
    public static class RunTestFixture
    {
        public static ObjectDatabaseSO CreateObjectDatabase()
        {
            var spaceNodes = UnityEditor.AssetDatabase.LoadAssetAtPath<SpaceNodeDatabaseSO>(
                "Assets/_Project/Runtime/Content/SpaceNodeDatabase.asset");
            var planets = UnityEditor.AssetDatabase.LoadAssetAtPath<PlanetDatabaseSO>(
                "Assets/_Project/Runtime/Content/PlanetDatabase.asset");
            var database = ScriptableObject.CreateInstance<ObjectDatabaseSO>();
            database.AssignDatabases(spaceNodes, planets);
            return database;
        }

        public static RunManager CreateRunManager(ObjectDatabaseSO objectDatabase = null, EventJournal journal = null)
        {
            objectDatabase = objectDatabase ?? CreateObjectDatabase();
            journal = journal ?? new EventJournal();
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            return new RunManager(timelineFactory, loader, objectDatabase, journal);
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
            var objectDatabase = CreateObjectDatabase();
            var journal = new EventJournal();
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            var manager = new RunManager(timelineFactory, loader, objectDatabase, journal);
            var route = new RouteService();
            var archive = new ArchiveService();
            var exploration = new ExplorationService(archive, route, journal, clock);
            var sensor = new SensorService(archive, objectDatabase, journal, clock);
            var travel = new TravelService(route, clock);
            var visibility = new VisibilityService();
            var travelSystem = new TravelSystem(manager, journal);
            loop.Register(travelSystem);
            return (manager, exploration, sensor, travel, visibility, travelSystem, loop, clock, journal);
        }
    }
}
