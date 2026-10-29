using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Overworld.Systems;
using ProjectStarwatch.Fleet;
using ProjectStarwatch.Research;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// Services wired the way the overworld plays: travel resolves, then the economy settles.
    /// </summary>
    internal sealed class OverworldTestContext
    {
        public RunManager Manager { get; }

        /// <summary>
        /// Same instance as <see cref="Manager"/>.ObjectDatabase — use after CreateRun for planet lookups.
        /// </summary>
        public ObjectDatabaseSO ObjectDatabase { get; }

        public ExplorationService ExplorationService { get; }

        public SensorService SensorService { get; }

        public TravelService TravelService { get; }

        public VisibilityService VisibilityService { get; }

        public TravelSystem TravelSystem { get; }

        public SimulationLoop Loop { get; }

        public SimulationClock Clock { get; }

        public EventJournal Journal { get; }

        public DomainEventBus Events { get; }

        public SettlementService SettlementService { get; }

        public BuildingService BuildingService { get; }

        public EconomyService EconomyService { get; }

        public ResearchService ResearchService { get; }

        public EffectRegistry Effects { get; }

        public FleetService FleetService { get; }

        public ShipProductionService ShipProductionService { get; }

        public OverworldTestContext(
            RunManager manager,
            ObjectDatabaseSO objectDatabase,
            ExplorationService explorationService,
            SensorService sensorService,
            TravelService travelService,
            VisibilityService visibilityService,
            TravelSystem travelSystem,
            SimulationLoop loop,
            SimulationClock clock,
            EventJournal journal,
            DomainEventBus events,
            SettlementService settlementService,
            BuildingService buildingService,
            EconomyService economyService,
            ResearchService researchService,
            EffectRegistry effects,
            FleetService fleetService,
            ShipProductionService shipProductionService)
        {
            Manager = manager;
            ObjectDatabase = objectDatabase;
            ExplorationService = explorationService;
            SensorService = sensorService;
            TravelService = travelService;
            VisibilityService = visibilityService;
            TravelSystem = travelSystem;
            Loop = loop;
            Clock = clock;
            Journal = journal;
            Events = events;
            SettlementService = settlementService;
            BuildingService = buildingService;
            EconomyService = economyService;
            ResearchService = researchService;
            Effects = effects;
            FleetService = fleetService;
            ShipProductionService = shipProductionService;
        }
    }
}
