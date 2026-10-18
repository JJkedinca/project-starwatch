using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Effects;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.Systems;
using EndlessHeros.Research;
using EndlessHeros.Settlement;
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
            var starSystems = UnityEditor.AssetDatabase.LoadAssetAtPath<StarSystemDatabaseSO>(
                "Assets/_Project/ScriptableObjects/StarSystemDatabase.asset");
            var planets = UnityEditor.AssetDatabase.LoadAssetAtPath<PlanetDatabaseSO>(
                "Assets/_Project/ScriptableObjects/PlanetDatabase.asset");
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<SettlementCatalogSO>(
                "Assets/_Project/ScriptableObjects/SettlementCatalog.asset");
            var research = UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchCatalogSO>(
                "Assets/_Project/ScriptableObjects/ResearchCatalog.asset");
            var database = ScriptableObject.CreateInstance<ObjectDatabaseSO>();
            database.AssignDatabases(starSystems, planets, catalog, research);
            return database;
        }

        internal static RunManager CreateRunManager(ObjectDatabaseSO objectDatabase = null, EventJournal journal = null)
        {
            objectDatabase = objectDatabase ?? CreateObjectDatabase();
            journal = journal ?? new EventJournal();
            var timelineFactory = new TimelineFactory(objectDatabase);
            var loader = new UniverseLoader(timelineFactory);
            return new RunManager(timelineFactory, loader, objectDatabase, journal, new SimulationLoop(new SimulationClock()));
        }

        internal static OverworldTestContext CreateOverworldStack()
        {
            var objectDatabase = CreateObjectDatabase();
            var journal = new EventJournal();
            var events = new DomainEventBus(journal);
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var timelineFactory = new TimelineFactory(objectDatabase);
            var loader = new UniverseLoader(timelineFactory);
            var manager = new RunManager(timelineFactory, loader, objectDatabase, journal, loop);
            var route = new RouteService();
            var archive = new ArchiveService();
            var exploration = new ExplorationService(archive, route, events, clock);
            var travel = new TravelService(route, clock, events);
            var visibility = new VisibilityService();
            var travelSystem = new TravelSystem(exploration, events);
            var effects = new EffectRegistry();
            var production = new ProductionService(objectDatabase, effects);
            var economy = new EconomyService(objectDatabase, production, events, clock);
            var research = new ResearchService(objectDatabase, effects, events, clock);
            var economyTick = new EconomyTickSystem(economy);
            var researchTick = new ResearchTickSystem(research, economy, economyTick);
            var sensor = new SensorService(archive, objectDatabase, events, clock);
            var settlement = new SettlementService(objectDatabase, events, clock);
            var buildings = new BuildingService(objectDatabase, events, clock);
            loop.Register(travelSystem);
            loop.Register(economyTick);
            loop.Register(researchTick);
            return new OverworldTestContext(
                manager: manager,
                exploration: exploration,
                sensor: sensor,
                travel: travel,
                visibility: visibility,
                travelSystem: travelSystem,
                loop: loop,
                clock: clock,
                journal: journal,
                events: events,
                settlement: settlement,
                buildings: buildings,
                economy: economy,
                research: research,
                effects: effects);
        }
    }
}
