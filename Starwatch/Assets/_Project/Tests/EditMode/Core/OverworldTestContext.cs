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

        public ExplorationService Exploration { get; }

        public SensorService Sensor { get; }

        public TravelService Travel { get; }

        public VisibilityService Visibility { get; }

        public TravelSystem TravelSystem { get; }

        public SimulationLoop Loop { get; }

        public SimulationClock Clock { get; }

        public EventJournal Journal { get; }

        public DomainEventBus Events { get; }

        public SettlementService Settlement { get; }

        public BuildingService Buildings { get; }

        public EconomyService Economy { get; }

        public ResearchService Research { get; }

        public EffectRegistry Effects { get; }

        public FleetService Fleets { get; }

        public ShipProductionService ShipProduction { get; }

        public OverworldTestContext(
            RunManager manager,
            ExplorationService exploration,
            SensorService sensor,
            TravelService travel,
            VisibilityService visibility,
            TravelSystem travelSystem,
            SimulationLoop loop,
            SimulationClock clock,
            EventJournal journal,
            DomainEventBus events,
            SettlementService settlement,
            BuildingService buildings,
            EconomyService economy,
            ResearchService research,
            EffectRegistry effects,
            FleetService fleets,
            ShipProductionService shipProduction)
        {
            Manager = manager;
            Exploration = exploration;
            Sensor = sensor;
            Travel = travel;
            Visibility = visibility;
            TravelSystem = travelSystem;
            Loop = loop;
            Clock = clock;
            Journal = journal;
            Events = events;
            Settlement = settlement;
            Buildings = buildings;
            Economy = economy;
            Research = research;
            Effects = effects;
            Fleets = fleets;
            ShipProduction = shipProduction;
        }
    }
}
