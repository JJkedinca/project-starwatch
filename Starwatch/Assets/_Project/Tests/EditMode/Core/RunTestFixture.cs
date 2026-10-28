using System;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Generation;
using ProjectStarwatch.Core.Identifiers;
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
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<SettlementCatalogSO>(
                "Assets/_Project/ScriptableObjects/Data/SettlementCatalog.asset");
            var research = UnityEditor.AssetDatabase.LoadAssetAtPath<ResearchCatalogSO>(
                "Assets/_Project/ScriptableObjects/Data/ResearchCatalog.asset");
            var ships = UnityEditor.AssetDatabase.LoadAssetAtPath<ShipCatalogSO>(
                "Assets/_Project/ScriptableObjects/Data/ShipCatalog.asset");
            var galaxyCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<GalaxyCatalogSO>(
                "Assets/_Project/ScriptableObjects/Map/Generation/GalaxyCatalog.asset");
            var planets = UnityEditor.AssetDatabase.LoadAssetAtPath<PlanetDatabaseSO>(
                "Assets/_Project/ScriptableObjects/Data/PlanetDatabase.asset");
            var starSystems = UnityEditor.AssetDatabase.LoadAssetAtPath<StarSystemDatabaseSO>(
                "Assets/_Project/ScriptableObjects/Data/StarSystemDatabase.asset");
            var database = ScriptableObject.CreateInstance<ObjectDatabaseSO>();
            database.AssignDatabases(catalog, research, ships, galaxyCatalog, planets, starSystems);
            return database;
        }

        internal static GalaxyGenerator CreateGalaxyGenerator(ObjectDatabaseSO objectDatabase = null)
        {
            objectDatabase = objectDatabase ?? CreateObjectDatabase();
            return new GalaxyGenerator(
                objectDatabase.GalaxyCatalog,
                objectDatabase.PlanetDatabase,
                objectDatabase.StarSystemDatabase);
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
                CreateGalaxyGenerator(objectDatabase),
                journal,
                new DomainEventBus(journal),
                new SimulationLoop(new SimulationClock()));
        }

        internal static OverworldTestContext CreateOverworldStack(ObjectDatabaseSO objectDatabase = null)
        {
            objectDatabase = objectDatabase ?? CreateObjectDatabase();
            var journal = new EventJournal();
            var events = new DomainEventBus(journal);
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var timelineFactory = new TimelineFactory();
            var loader = new UniverseLoader(timelineFactory);
            var manager = new RunManager(
                timelineFactory,
                loader,
                objectDatabase,
                CreateGalaxyGenerator(objectDatabase),
                journal,
                events,
                loop);
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
                objectDatabase: objectDatabase,
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

        internal static RunState CreateStandardRun(RunManager manager, int seed = 42)
            => manager.CreateRun(GalaxyGenerator.ScenarioId, RunSeed.FromValue(seed));

        internal static ContentId HomeSystem(RunState run)
            => run.Universe.StartingEmpire.HomeSystemId;

        internal static ContentId HomePlanet(RunState run)
        {
            var settlements = run.Universe.StartingEmpire.StartingSettlements;
            if (settlements != null && settlements.Count > 0 && !settlements[0].PlanetId.IsEmpty)
            {
                return settlements[0].PlanetId;
            }

            if (run.Universe.MapGraph.TryGetNode(HomeSystem(run), out var node) && node.PlanetIds.Count > 0)
            {
                return node.PlanetIds[0];
            }

            return default;
        }

        internal static ContentId FirstNeighbor(RunState run, ContentId systemId = default)
        {
            if (systemId.IsEmpty)
            {
                systemId = HomeSystem(run);
            }

            var neighbors = run.Universe.MapGraph.GetNeighbors(systemId);
            return neighbors.Count > 0 ? neighbors[0] : default;
        }

        internal static ContentId FirstPlanetAt(RunState run, ContentId systemId)
        {
            if (!run.Universe.MapGraph.TryGetNode(systemId, out var node) || node.PlanetIds.Count == 0)
            {
                return default;
            }

            return node.PlanetIds[0];
        }

        internal static StartingEmpireDefinition UnsettledGeneratedEmpire(int seed = 42)
        {
            var manager = CreateRunManager();
            var run = CreateStandardRun(manager, seed);
            return run.Universe.StartingEmpire.WithoutSettlements();
        }
    }
}
