using System;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Settlement
{
    /// <summary>
    /// EditMode coverage for outposts, the settlement curve, buildings, and the economy tick.
    /// </summary>
    public sealed class SettlementLoopTests
    {
        [Test]
        public void FoundColony_SpendsRisingCostAndRejectsASecondColonyTheStockpileCannotAfford()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 51);
            var home = RunTestFixture.HomePlanet(run);
            var moon = RunTestFixture.SecondPlanetAt(run, RunTestFixture.HomeSystem(run));
            Assert.That(moon.IsEmpty, Is.False);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, moon);

            var first = stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            var blocked = stack.SettlementService.FoundColony(run, ObserverId.Player, moon);

            Assert.That(first.Success, Is.True);
            Assert.That(run.Timeline.ColonyCount(), Is.EqualTo(1));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(40));
            Assert.That(blocked.Success, Is.False);
            Assert.That(blocked.Error.Code, Is.EqualTo(CommandErrorCode.InsufficientResources));
            Assert.That(run.Timeline.ColonyCount(), Is.EqualTo(1));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(40));
        }

        [Test]
        public void FoundOutpost_DoesNotCountTowardTheColonyCurve()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 52);
            var home = RunTestFixture.HomePlanet(run);
            var moon = RunTestFixture.SecondPlanetAt(run, RunTestFixture.HomeSystem(run));
            Assert.That(moon.IsEmpty, Is.False);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, moon);

            var outpost = stack.SettlementService.FoundOutpost(run, ObserverId.Player, moon);

            Assert.That(outpost.Success, Is.True);
            Assert.That(run.Timeline.ColonyCount(), Is.EqualTo(0));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(70));

            var colony = stack.SettlementService.FoundColony(run, ObserverId.Player, home);

            Assert.That(colony.Success, Is.True);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(30));
            Assert.That(run.Timeline.TryGetColony(moon, out var claim), Is.True);
            Assert.That(claim.Presence, Is.EqualTo(ColonyPresence.Outpost));
        }

        [Test]
        public void FoundColony_PromotesAnOutpostAndRequiresAScan()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 53);
            var home = RunTestFixture.HomePlanet(run);
            var forge = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));

            var unscanned = stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            var outpost = stack.SettlementService.FoundOutpost(run, ObserverId.Player, home);
            var colony = stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            var hiddenForge = stack.SettlementService.FoundOutpost(run, ObserverId.Player, forge);

            Assert.That(unscanned.Error.Code, Is.EqualTo(CommandErrorCode.PrerequisiteNotMet));
            Assert.That(outpost.Success, Is.True);
            Assert.That(colony.Success, Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var claim), Is.True);
            Assert.That(claim.Presence, Is.EqualTo(ColonyPresence.Colony));
            Assert.That(hiddenForge.Success, Is.False);
        }

        [Test]
        public void Place_AllowsEveryCatalogBuildingWithinTheFootprint()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            // Home footprint is at least GalaxyHomeFootprint.Minimum; barren neighbors can be too tight.
            var run = UnsettledRun(stack, 54);
            var home = RunTestFixture.HomePlanet(run);
            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            run.Timeline.Resources.Grant(new[]
            {
                new ResourceAmount(ResourceKind.Industry, 120),
            });

            var industry = ContentId.From("industry-works");
            var lab = ContentId.From("laboratory");
            var power = ContentId.From("power-plant");
            var first = stack.BuildingService.Place(run, home, industry);
            var offSpec = stack.BuildingService.Place(run, home, lab);
            var powerPlant = stack.BuildingService.Place(run, home, power);
            var extractor = stack.BuildingService.Place(run, home, ContentId.From("extractor"));

            Assert.That(first.Success, Is.True);
            Assert.That(offSpec.Success, Is.True);
            Assert.That(powerPlant.Success, Is.True);
            Assert.That(extractor.Success, Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            Assert.That(colony.Buildings, Has.Count.EqualTo(4));
        }

        [Test]
        public void Upgrade_StopsAtMaxLevel()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 55);
            var home = RunTestFixture.HomePlanet(run);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            var industry = ContentId.From("industry-works");

            Assert.That(stack.BuildingService.Place(run, home, industry).Success, Is.True);
            Assert.That(stack.BuildingService.Upgrade(run, home, industry).Success, Is.True);
            var capped = stack.BuildingService.Upgrade(run, home, industry);

            Assert.That(capped.Success, Is.False);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            Assert.That(colony.FindBuilding(industry).Level, Is.EqualTo(2));
        }

        [Test]
        public void EconomyTick_SettlesAccumulatedRatesOncePerInterval()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 56);
            var home = RunTestFixture.HomePlanet(run);
            var forge = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            var industry = ContentId.From("industry-works");
            Assert.That(stack.BuildingService.Place(run, home, industry).Success, Is.True);
            Assert.That(stack.ObjectDatabase.TryGetPlanet(home.Value, out var homePlanet), Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var homeColony), Is.True);
            var homeQuote = new ProductionService(stack.ObjectDatabase).Quote(homeColony, homePlanet);
            var homeIndustryRate = homeQuote.Output.Get(ResourceKind.Industry);
            var homeEnergyUpkeep = homeQuote.Upkeep.Get(ResourceKind.Energy);
            var industryBefore = run.Timeline.Resources.Get(ResourceKind.Industry);
            var energyBefore = run.Timeline.Resources.Get(ResourceKind.Energy);
            var ticksPerInterval = TicksPerEconomyStep();
            var seconds = EconomyTickSystem.kIntervalSeconds;
            var notices = ResourceChangeCount(stack.Journal);
            var industryRemainder = 0f;

            AdvanceTicks(stack.Loop, stack.Clock, ticksPerInterval - 1);
            var creditedBeforeClose = FlooredCredit(homeIndustryRate, seconds - 1, ref industryRemainder);

            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Industry),
                Is.EqualTo(industryBefore + creditedBeforeClose));
            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Energy),
                Is.EqualTo(energyBefore - (homeEnergyUpkeep * (seconds - 1))).Within(0.01f));
            Assert.That(ResourceChangeCount(stack.Journal), Is.EqualTo(notices));

            AdvanceTicks(stack.Loop, stack.Clock, 1);
            var creditedAtClose = FlooredCredit(homeIndustryRate, 1, ref industryRemainder);

            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Industry),
                Is.EqualTo(industryBefore + creditedBeforeClose + creditedAtClose));
            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Energy),
                Is.EqualTo(energyBefore - (homeEnergyUpkeep * seconds)).Within(0.01f));
            Assert.That(ResourceChangeCount(stack.Journal), Is.EqualTo(notices + 1));

            PrepareForge(stack.ExplorationService, stack.SensorService, run, forge);
            run.Timeline.Resources.Grant(new[]
            {
                new ResourceAmount(ResourceKind.Industry, 120),
                new ResourceAmount(ResourceKind.Energy, 10),
            });
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, forge).Success, Is.True);
            Assert.That(stack.BuildingService.Place(run, forge, industry).Success, Is.True);
            Assert.That(stack.BuildingService.Place(run, forge, ContentId.From("extractor")).Success, Is.True);
            Assert.That(stack.ObjectDatabase.TryGetPlanet(forge.Value, out var forgePlanet), Is.True);
            Assert.That(run.Timeline.TryGetColony(forge, out var forgeColony), Is.True);
            var forgeQuote = new ProductionService(stack.ObjectDatabase).Quote(forgeColony, forgePlanet);
            var combinedIndustry =
                homeIndustryRate + forgeQuote.Output.Get(ResourceKind.Industry);
            var beforeForgeTick = run.Timeline.Resources.Get(ResourceKind.Industry);
            var strategicBefore = run.Timeline.Resources.Get(ResourceKind.Strategic);
            var strategicRemainder = 0f;
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerInterval);
            var forgeIndustryCredit = FlooredCredit(combinedIndustry, seconds, ref industryRemainder);
            var forgeStrategicCredit = FlooredCredit(
                forgeQuote.Output.Get(ResourceKind.Strategic),
                seconds,
                ref strategicRemainder);

            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Industry),
                Is.EqualTo(beforeForgeTick + forgeIndustryCredit));
            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Strategic),
                Is.EqualTo(strategicBefore + forgeStrategicCredit));
        }

        [Test]
        public void EconomyTick_AppliesResearchScienceEverySecond()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 57);
            var home = RunTestFixture.HomePlanet(run);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            Assert.That(stack.BuildingService.Place(run, home, ContentId.From("laboratory")).Success, Is.True);
            var offered = run.Timeline.Research.Offers[0];
            Assert.That(stack.ResearchService.Start(run, offered).Success, Is.True);
            var notices = ResourceChangeCount(stack.Journal);

            var ticksPerSecond = (int)Math.Round(1f / SimulationLoop.kTickIntervalSeconds);
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerSecond);

            Assert.That(run.Timeline.Research.Points, Is.GreaterThan(0f));
            Assert.That(run.Timeline.Research.Points, Is.EqualTo(stack.EconomyService.ProducedScience));

            var afterOneSecond = run.Timeline.Research.Points;
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerSecond);

            Assert.That(run.Timeline.Research.Points, Is.EqualTo(afterOneSecond + stack.EconomyService.ProducedScience));
            Assert.That(ResourceChangeCount(stack.Journal), Is.EqualTo(notices));
        }

        [Test]
        public void EconomyTick_CreditsOnlyTheSecondsABuildingExisted()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 66);
            var home = RunTestFixture.HomePlanet(run);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            stack.SettlementService.FoundColony(run, ObserverId.Player, home);
            var ticksPerSecond = (int)Math.Round(1f / SimulationLoop.kTickIntervalSeconds);
            var seconds = EconomyTickSystem.kIntervalSeconds;
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerSecond * (seconds - 1));
            var notices = ResourceChangeCount(stack.Journal);

            Assert.That(stack.BuildingService.Place(run, home, ContentId.From("industry-works")).Success, Is.True);
            Assert.That(stack.ObjectDatabase.TryGetPlanet(home.Value, out var planet), Is.True);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            var industryRate = new ProductionService(stack.ObjectDatabase)
                .Quote(colony, planet)
                .Output.Get(ResourceKind.Industry);
            var industryBefore = run.Timeline.Resources.Get(ResourceKind.Industry);
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerSecond);

            var industryRemainder = 0f;
            Assert.That(
                run.Timeline.Resources.Get(ResourceKind.Industry),
                Is.EqualTo(industryBefore + FlooredCredit(industryRate, 1, ref industryRemainder)));
            Assert.That(ResourceChangeCount(stack.Journal), Is.EqualTo(notices + 1));
        }

        [Test]
        public void FoundOutpost_RejectsAPlanetMissingFromTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var source = RunTestFixture.CreateStandardRun(stack.Manager, 58);
            var home = RunTestFixture.HomeSystem(source);
            var planet = RunTestFixture.HomePlanet(source);
            var (settlementService, run) = CreateRunOnGraph(
                stack.ObjectDatabase,
                new MapNode(home, MapNodeKind.System, Vector2.zero, Array.Empty<ContentId>()),
                home);

            var result = settlementService.FoundOutpost(run, ObserverId.Player, planet);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(run.Timeline.TryGetColony(planet, out _), Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
        }

        [Test]
        public void FoundOutpost_RejectsAPlanetWhoseHostDoesNotMatchTheMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var source = RunTestFixture.CreateStandardRun(stack.Manager, 58);
            var planet = RunTestFixture.HomePlanet(source);
            var home = RunTestFixture.HomeSystem(source);
            var listedOn = ContentId.From("other-system");
            var (settlementService, run) = CreateRunOnGraph(
                stack.ObjectDatabase,
                new MapNode(listedOn, MapNodeKind.System, Vector2.zero, new[] { planet }),
                listedOn,
                home);

            var result = settlementService.FoundOutpost(run, ObserverId.Player, planet);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(run.Timeline.TryGetColony(planet, out _), Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
        }

        [Test]
        public void Abandon_DropsTheClaimAndLeavesTheSunkCostSpent()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 58);
            var home = RunTestFixture.HomePlanet(run);
            var missing = stack.SettlementService.Abandon(run, home);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            var founded = stack.SettlementService.FoundColony(run, ObserverId.Player, home);

            var abandoned = stack.SettlementService.Abandon(run, home);
            var abandonedRecord = stack.Journal.Records[stack.Journal.Records.Count - 1];
            var again = stack.SettlementService.FoundColony(run, ObserverId.Player, home);

            Assert.That(missing.Success, Is.False);
            Assert.That(missing.Error.Code, Is.EqualTo(CommandErrorCode.NotAllowed));
            Assert.That(founded.Success, Is.True);
            Assert.That(abandoned.Success, Is.True);
            Assert.That(again.Success, Is.True);
            Assert.That(run.Timeline.ColonyCount(), Is.EqualTo(1));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(0));
            Assert.That(abandonedRecord.EventType, Is.EqualTo(GameEventType.SettlementAbandoned));
            Assert.That(abandonedRecord.TryGet(out SettlementAbandonedEvent abandonedEvent), Is.True);
            Assert.That(abandonedEvent.PlanetId, Is.EqualTo(home));
            Assert.That(abandonedEvent.Kind, Is.EqualTo(SettlementKind.Colony));
        }

        [Test]
        public void NewTimeline_DropsColoniesAndRestoresStartingResources()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 57);
            var home = RunTestFixture.HomePlanet(run);
            var moon = RunTestFixture.SecondPlanetAt(run, RunTestFixture.HomeSystem(run));
            Assert.That(moon.IsEmpty, Is.False);
            Assert.That(stack.SensorService.ScanPlanet(run, ObserverId.Player, moon).Success, Is.True);
            Assert.That(stack.SettlementService.FoundOutpost(run, ObserverId.Player, moon).Success, Is.True);

            var fresh = new TimelineFactory().Create(run.Universe, run.Archive, new EntityIdFactory());

            Assert.That(fresh.TryGetColony(home, out var restored), Is.True);
            Assert.That(restored.Presence, Is.EqualTo(ColonyPresence.Colony));
            Assert.That(fresh.TryGetColony(moon, out _), Is.False);
            Assert.That(fresh.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(run.Timeline.TryGetColony(moon, out _), Is.True);
        }

        [Test]
        public void EconomyTick_DropsThePartialSecondWhenTheTimelineIsRecreated()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 59);
            var home = RunTestFixture.HomePlanet(run);
            var industry = ContentId.From("industry-works");
            stack.SensorService.ScanPlanet(run, ObserverId.Player, home);
            Assert.That(stack.SettlementService.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.BuildingService.Place(run, home, industry).Success, Is.True);
            AdvanceTicks(stack.Loop, stack.Clock, TicksPerEconomyStep() - 1);

            var timeline = stack.Manager.RecreateTimeline(1);
            Assert.That(timeline, Is.SameAs(run.Timeline));
            var industryAfter = run.Timeline.Resources.Get(ResourceKind.Industry);
            var energyAfter = run.Timeline.Resources.Get(ResourceKind.Energy);

            AdvanceTicks(stack.Loop, stack.Clock, 1);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industryAfter));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(energyAfter));
        }

        [Test]
        public void GeneratedGalaxy_StartsWithALevelOneHomeColonyAndOneOfEachBuilding()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 60);
            var home = RunTestFixture.HomePlanet(run);
            var catalog = stack.ObjectDatabase.SettlementCatalog;

            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            Assert.That(colony.Presence, Is.EqualTo(ColonyPresence.Colony));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
            var producingCount = 0;
            for (var i = 0; i < catalog.Buildings.Count; i++)
            {
                var definition = catalog.Buildings[i];
                var placed = colony.FindBuilding(definition.Id);
                var produces = false;
                for (var rate = 0; rate < definition.Output.Count; rate++)
                {
                    if (definition.Output[rate].Amount > 0f)
                    {
                        produces = true;
                        break;
                    }
                }

                if (!produces)
                {
                    Assert.That(placed, Is.Null);
                    continue;
                }

                producingCount++;
                Assert.That(placed.Level, Is.EqualTo(1));
            }

            Assert.That(colony.Buildings, Has.Count.EqualTo(producingCount));
        }

        private static RunState UnsettledRun(OverworldTestContext stack, int seed)
            => RunTestFixture.CreateUnsettledRun(stack.Manager, seed);

        private static (SettlementService SettlementService, RunState Run) CreateRunOnGraph(
            ObjectDatabaseSO objectDatabase,
            MapNode node,
            params ContentId[] exploredNodeIds)
        {
            var settlementService = new SettlementService(
                objectDatabase,
                new DomainEventBus(new EventJournal()),
                new SimulationClock());
            Assert.That(MapGraph.TryCreate(new[] { node }, Array.Empty<MapRoute>(), out var graph, out _), Is.True);
            var universe = new UniverseState(
                "graph-claim",
                new StartingEmpireDefinition(node.Id, ContentId.From("starter-screen"), 1),
                graph);
            var archive = new RunArchive();
            for (var i = 0; i < exploredNodeIds.Length; i++)
            {
                archive.TryAddExploredNode(exploredNodeIds[i]);
            }

            var entityIds = new EntityIdFactory();
            var timeline = new TimelineState(entityIds);
            timeline.Resources.Grant(new StartingResources(industry: 80, energy: 40));
            var run = new RunState(
                RunSeed.FromValue(58),
                universe,
                timeline,
                archive,
                rewindIndex: 0,
                new DeterministicRandom(58),
                entityIds);
            return (settlementService, run);
        }

        private static int ResourceChangeCount(EventJournal journal)
        {
            var count = 0;
            var records = journal.Records;
            for (var i = 0; i < records.Count; i++)
            {
                if (records[i].EventType == GameEventType.ResourceChanged)
                {
                    count++;
                }
            }

            return count;
        }

        private static int TicksPerEconomyStep()
        {
            return (int)Math.Round(EconomyTickSystem.kIntervalSeconds / (double)SimulationLoop.kTickIntervalSeconds);
        }

        /// <summary>
        /// Matches <see cref="ResourceInventory.DepositFlooredInto"/> + RetainFraction across settled seconds.
        /// </summary>
        private static int FlooredCredit(float ratePerSecond, int seconds, ref float remainder)
        {
            var credited = 0;
            for (var i = 0; i < seconds; i++)
            {
                remainder += ratePerSecond;
                var whole = (int)MathF.Floor(remainder);
                credited += whole;
                remainder -= whole;
            }

            return credited;
        }

        private static void AdvanceTicks(SimulationLoop loop, SimulationClock clock, int ticks)
        {
            clock.SetPaused(false);
            for (var i = 0; i < ticks; i++)
            {
                loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }
        }

        private static void PrepareForge(ExplorationService explorationService, SensorService sensorService, RunState run, ContentId forgePlanet)
        {
            var fleetId = run.Timeline.Fleets[0].Id;
            run.Timeline.Fleets.TrySetFleetLocation(fleetId, RunTestFixture.FirstNeighbor(run));
            explorationService.Explore(run, ObserverId.Player, RunTestFixture.FirstNeighbor(run));
            sensorService.ScanPlanet(run, ObserverId.Player, forgePlanet);
        }
    }
}
