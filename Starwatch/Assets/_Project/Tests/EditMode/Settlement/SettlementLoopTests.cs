using System;
using EndlessHeros.Core.Commands;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Archive;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Settlement;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Settlement
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
            var home = ContentId.From("home-prime");
            var moon = ContentId.From("home-moon");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            stack.Sensor.ScanPlanet(run, ObserverId.Player, moon);

            var first = stack.Settlement.FoundColony(run, ObserverId.Player, home);
            var blocked = stack.Settlement.FoundColony(run, ObserverId.Player, moon);

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
            var moon = ContentId.From("home-moon");
            var home = ContentId.From("home-prime");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);

            var outpost = stack.Settlement.FoundOutpost(run, ObserverId.Player, moon);
            var colony = stack.Settlement.FoundColony(run, ObserverId.Player, home);

            Assert.That(outpost.Success, Is.True);
            Assert.That(run.Timeline.ColonyCount(), Is.EqualTo(0));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(70));
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
            var home = ContentId.From("home-prime");
            var forge = ContentId.From("forge-world");

            var unscanned = stack.Settlement.FoundColony(run, ObserverId.Player, home);
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            var outpost = stack.Settlement.FoundOutpost(run, ObserverId.Player, home);
            var colony = stack.Settlement.FoundColony(run, ObserverId.Player, home);
            var hiddenForge = stack.Settlement.FoundOutpost(run, ObserverId.Player, forge);

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
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(54));
            var forge = ContentId.From("forge-world");
            PrepareForge(stack.Exploration, stack.Sensor, run, forge);
            stack.Settlement.FoundColony(run, ObserverId.Player, forge);

            var industry = ContentId.From("industry-works");
            var lab = ContentId.From("laboratory");
            var power = ContentId.From("power-plant");
            var first = stack.Buildings.Place(run, forge, industry);
            var offSpec = stack.Buildings.Place(run, forge, lab);
            var powerPlant = stack.Buildings.Place(run, forge, power);
            var extractor = stack.Buildings.Place(run, forge, ContentId.From("extractor"));

            Assert.That(first.Success, Is.True);
            Assert.That(offSpec.Success, Is.True);
            Assert.That(powerPlant.Success, Is.True);
            Assert.That(extractor.Success, Is.True);
            Assert.That(run.Timeline.TryGetColony(forge, out var colony), Is.True);
            Assert.That(colony.Buildings, Has.Count.EqualTo(4));
        }

        [Test]
        public void Upgrade_StopsAtMaxLevel()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 55);
            var home = ContentId.From("home-prime");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            stack.Settlement.FoundColony(run, ObserverId.Player, home);
            var industry = ContentId.From("industry-works");

            Assert.That(stack.Buildings.Place(run, home, industry).Success, Is.True);
            Assert.That(stack.Buildings.Upgrade(run, home, industry).Success, Is.True);
            var capped = stack.Buildings.Upgrade(run, home, industry);

            Assert.That(capped.Success, Is.False);
            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            Assert.That(colony.FindBuilding(industry).Level, Is.EqualTo(2));
        }

        [Test]
        public void EconomyTick_SettlesAccumulatedRatesOncePerInterval()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = UnsettledRun(stack, 56);
            var home = ContentId.From("home-prime");
            var forge = ContentId.From("forge-world");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            stack.Settlement.FoundColony(run, ObserverId.Player, home);
            var industry = ContentId.From("industry-works");
            stack.Buildings.Place(run, home, industry);
            var industryBefore = run.Timeline.Resources.Get(ResourceKind.Industry);
            var energyBefore = run.Timeline.Resources.Get(ResourceKind.Energy);
            var ticksPerInterval = TicksPerEconomyStep();
            var seconds = (int)EconomyTickSystem.kIntervalSeconds;

            AdvanceTicks(stack.Loop, stack.Clock, ticksPerInterval - 1);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industryBefore));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(energyBefore));

            AdvanceTicks(stack.Loop, stack.Clock, 1);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industryBefore + (2 * seconds)));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Energy), Is.EqualTo(energyBefore));

            PrepareForge(stack.Exploration, stack.Sensor, run, forge);
            run.Timeline.Resources.Grant(new[]
            {
                new ResourceAmount(ResourceKind.Industry, 120),
                new ResourceAmount(ResourceKind.Energy, 10),
            });
            Assert.That(stack.Settlement.FoundColony(run, ObserverId.Player, forge).Success, Is.True);
            Assert.That(stack.Buildings.Place(run, forge, industry).Success, Is.True);
            Assert.That(stack.Buildings.Place(run, forge, ContentId.From("extractor")).Success, Is.True);
            var beforeForgeTick = run.Timeline.Resources.Get(ResourceKind.Industry);
            var strategicBefore = run.Timeline.Resources.Get(ResourceKind.Strategic);
            AdvanceTicks(stack.Loop, stack.Clock, ticksPerInterval);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(beforeForgeTick + ((2 + 4) * seconds)));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Strategic), Is.EqualTo(strategicBefore + (4 * seconds)));
        }

        [Test]
        public void FoundOutpost_RejectsAPlanetMissingFromTheMap()
        {
            var home = ContentId.From("home-system");
            var planet = ContentId.From("home-prime");
            var (settlement, run) = CreateRunOnGraph(
                new MapNode(home, MapNodeKind.System, Vector2.zero, Array.Empty<ContentId>()),
                home);

            var result = settlement.FoundOutpost(run, ObserverId.Player, planet);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(run.Timeline.TryGetColony(planet, out _), Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
        }

        [Test]
        public void FoundOutpost_RejectsAPlanetWhoseHostDoesNotMatchTheMap()
        {
            var planet = ContentId.From("home-prime");
            var listedOn = ContentId.From("other-system");
            var (settlement, run) = CreateRunOnGraph(
                new MapNode(listedOn, MapNodeKind.System, Vector2.zero, new[] { planet }),
                listedOn,
                ContentId.From("home-system"));

            var result = settlement.FoundOutpost(run, ObserverId.Player, planet);

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
            var home = ContentId.From("home-prime");
            var missing = stack.Settlement.Abandon(run, home);
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            var founded = stack.Settlement.FoundColony(run, ObserverId.Player, home);

            var abandoned = stack.Settlement.Abandon(run, home);
            var abandonedRecord = stack.Journal.Records[stack.Journal.Records.Count - 1];
            var again = stack.Settlement.FoundColony(run, ObserverId.Player, home);

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
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(57));
            var home = ContentId.From("home-prime");
            var moon = ContentId.From("home-moon");
            Assert.That(stack.Settlement.FoundOutpost(run, ObserverId.Player, moon).Success, Is.True);

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
            var home = ContentId.From("home-prime");
            var industry = ContentId.From("industry-works");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, home);
            Assert.That(stack.Settlement.FoundColony(run, ObserverId.Player, home).Success, Is.True);
            Assert.That(stack.Buildings.Place(run, home, industry).Success, Is.True);
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
        public void VerticalSlice_StartsWithALevelOneHomeColonyAndOneOfEachBuilding()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(60));
            var home = ContentId.From("home-prime");
            var catalog = RunTestFixture.CreateObjectDatabase().SettlementCatalog;

            Assert.That(run.Timeline.TryGetColony(home, out var colony), Is.True);
            Assert.That(colony.Presence, Is.EqualTo(ColonyPresence.Colony));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(colony.Buildings, Has.Count.EqualTo(catalog.Buildings.Count));
            for (var i = 0; i < catalog.Buildings.Count; i++)
            {
                var definition = catalog.Buildings[i];
                var placed = colony.FindBuilding(definition.Id);
                Assert.That(placed, Is.Not.Null);
                Assert.That(placed.Level, Is.EqualTo(1));
            }
        }

        private static RunState UnsettledRun(OverworldTestContext stack, int seed)
        {
            return stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(seed),
                RunTestFixture.UnsettledVerticalSlice());
        }

        private static (SettlementService Settlement, RunState Run) CreateRunOnGraph(
            MapNode node,
            params ContentId[] exploredNodeIds)
        {
            var settlement = new SettlementService(
                RunTestFixture.CreateObjectDatabase(),
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
            return (settlement, run);
        }

        private static int TicksPerEconomyStep()
        {
            return (int)Math.Round(EconomyTickSystem.kIntervalSeconds / SimulationLoop.kTickIntervalSeconds);
        }

        private static void AdvanceTicks(SimulationLoop loop, SimulationClock clock, int ticks)
        {
            clock.SetPaused(false);
            for (var i = 0; i < ticks; i++)
            {
                loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }
        }

        private static void PrepareForge(ExplorationService exploration, SensorService sensor, RunState run, ContentId forgePlanet)
        {
            var fleetId = run.Timeline.Fleets[0].Id;
            run.Timeline.TrySetFleetLocation(fleetId, ContentId.From("forge-system"));
            exploration.Explore(run, ObserverId.Player, ContentId.From("forge-system"));
            sensor.ScanPlanet(run, ObserverId.Player, forgePlanet);
        }
    }
}
