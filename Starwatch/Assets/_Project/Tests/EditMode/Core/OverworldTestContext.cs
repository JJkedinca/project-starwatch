using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.Systems;
using EndlessHeros.Settlement;

namespace EndlessHeros.Tests.EditMode.Core
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
            EconomyService economy)
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
        }
    }
}
