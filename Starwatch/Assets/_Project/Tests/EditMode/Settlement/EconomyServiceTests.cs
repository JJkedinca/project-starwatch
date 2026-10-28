using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Settlement
{
    /// <summary>
    /// EditMode coverage for fractional output carried between economy seconds.
    /// </summary>
    public sealed class EconomyServiceTests
    {
        [Test]
        public void ApplyTick_CarriesAFractionalRateUntilItBecomesWhole()
        {
            var economyService = CreateEconomy();
            var run = CreateRun();
            var colony = new ColonyState(ContentId.From("fraction-world"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(ContentId.From("industry-works"), 1));
            Assert.That(run.Timeline.TryAddColony(colony), Is.True);
            run.Timeline.Resources.Grant(new StartingResources(energy: 2));

            economyService.ApplyTick(run);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(0));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(1));

            economyService.ApplyTick(run);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(1));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(0));
        }

        [Test]
        public void ApplyTick_UnpaidUpkeepDoesNotBankOutput()
        {
            var economyService = CreateEconomy();
            var run = CreateRun();
            var colony = new ColonyState(ContentId.From("fraction-world"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(ContentId.From("industry-works"), 1));
            Assert.That(run.Timeline.TryAddColony(colony), Is.True);

            economyService.ApplyTick(run);
            run.Timeline.Resources.Grant(new StartingResources(energy: 1));
            economyService.ApplyTick(run);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(0));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(0));
        }

        [Test]
        public void ApplyTick_TimelineResetDropsTheFractionalRemainder()
        {
            var economyService = CreateEconomy();
            var tick = new EconomyTickSystem(economyService);
            var run = CreateRun();
            var colony = new ColonyState(ContentId.From("fraction-world"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(ContentId.From("industry-works"), 1));
            Assert.That(run.Timeline.TryAddColony(colony), Is.True);
            run.Timeline.Resources.Grant(new StartingResources(energy: 2));

            economyService.ApplyTick(run);
            tick.OnTimelineReset();
            economyService.ApplyTick(run);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(0));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(0));
        }

        private static EconomyService CreateEconomy()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var scenario = ScriptableObject.CreateInstance<ScenarioDefinition>();
            scenario.Configure(
                "fraction-slice",
                StartingEmpireDefinition.CreateDefault(),
                System.Array.Empty<MapNodeDefinition>(),
                System.Array.Empty<MapRouteDefinition>(),
                new[]
                {
                    new PlanetDefinition(
                        ContentId.From("fraction-world"),
                        ContentId.From("fraction-system"),
                        "Fraction World",
                        "Half industry.",
                        footprintBudget: 4,
                        modifiers: new PlanetModifiers(industry: 0.25f, energy: 1f, science: 1f, strategic: 1f)),
                });
            database.RegisterScenario(scenario);
            return new EconomyService(
                database,
                new ProductionService(database),
                new DomainEventBus(new EventJournal()),
                new SimulationClock());
        }

        private static RunState CreateRun()
        {
            var entityIds = new EntityIdFactory();
            var timeline = new TimelineState(entityIds);
            return new RunState(
                RunSeed.FromValue(61),
                new UniverseState("fraction-slice", StartingEmpireDefinition.CreateDefault(), MapGraph.Empty),
                timeline,
                new RunArchive(),
                rewindIndex: 0,
                new DeterministicRandom(61),
                entityIds);
        }
    }
}
