using System;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Overworld.Systems;
using ProjectStarwatch.Fleet;
using ProjectStarwatch.Research;
using ProjectStarwatch.Settlement;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core
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
            var ships = UnityEditor.AssetDatabase.LoadAssetAtPath<ShipCatalogSO>(
                "Assets/_Project/ScriptableObjects/ShipCatalog.asset");
            var database = ScriptableObject.CreateInstance<ObjectDatabaseSO>();
            database.AssignDatabases(starSystems, planets, catalog, research, ships);
            return database;
        }

        internal static RunManager CreateRunManager(ObjectDatabaseSO objectDatabase = null, EventJournal journal = null)
        {
            objectDatabase = objectDatabase ?? CreateObjectDatabase();
            journal = journal ?? new EventJournal();
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            return new RunManager(
                timelineFactory,
                loader,
                objectDatabase,
                journal,
                new DomainEventBus(journal),
                new SimulationLoop(new SimulationClock()));
        }

        internal static OverworldTestContext CreateOverworldStack()
        {
            var objectDatabase = CreateObjectDatabase();
            var journal = new EventJournal();
            var events = new DomainEventBus(journal);
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            var manager = new RunManager(timelineFactory, loader, objectDatabase, journal, events, loop);
            var routeService = new RouteService();
            var archiveService = new ArchiveService();
            var effects = new EffectRegistry();
            var explorationService = new ExplorationService(archiveService, routeService, events, clock);
            var travelService = new TravelService(routeService, clock, events, effects, objectDatabase);
            var visibilityService = new VisibilityService();
            var travelSystem = new TravelSystem(explorationService, events);
            var productionService = new ProductionService(objectDatabase, effects);
            var economyService = new EconomyService(objectDatabase, productionService, events, clock);
            var researchService = new ResearchService(objectDatabase, effects, events, clock);
            var economyTick = new EconomyTickSystem(economyService);
            var researchTick = new ResearchTickSystem(researchService, economyTick);
            var sensorService = new SensorService(archiveService, objectDatabase, events, clock);
            var settlementService = new SettlementService(objectDatabase, events, clock);
            var buildingService = new BuildingService(objectDatabase, events, clock);
            var fleetService = new FleetService(events, clock);
            var shipProductionService = new ShipProductionService(objectDatabase, effects, events, clock);
            var shipProductionSystem = new ShipProductionSystem(objectDatabase, fleetService, shipProductionService, events);
            loop.Register(travelSystem);
            loop.Register(economyTick);
            loop.Register(researchTick);
            loop.Register(shipProductionSystem);
            return new OverworldTestContext(
                manager: manager,
                explorationService: explorationService,
                sensorService: sensorService,
                travelService: travelService,
                visibilityService: visibilityService,
                travelSystem: travelSystem,
                loop: loop,
                clock: clock,
                journal: journal,
                events: events,
                settlementService: settlementService,
                buildingService: buildingService,
                economyService: economyService,
                researchService: researchService,
                effects: effects,
                fleetService: fleetService,
                shipProductionService: shipProductionService);
        }

        internal static StartingEmpireDefinition UnsettledVerticalSlice()
        {
            var database = CreateObjectDatabase();
            if (!database.TryGetScenario(VerticalSliceScenarioFactory.ScenarioId, out var scenario)
                || scenario.StartingEmpire == null)
            {
                throw new InvalidOperationException("The vertical slice scenario is missing.");
            }

            return scenario.StartingEmpire.WithoutSettlements();
        }
    }
}
