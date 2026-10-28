using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using ProjectStarwatch.Core.Generation;
using UnityEngine.TestTools;

namespace ProjectStarwatch.Tests.EditMode.Fleet
{
    /// <summary>
    /// Ship construction, fleet composition, and travel time.
    /// </summary>
    public sealed class FleetLoopTests
    {
        [Test]
        public void Queue_SpendsCostAndFinishesIntoTheIdleFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 90);
            Unlock(stack, run, "basic-fleet");
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            var industry = run.Timeline.Resources.Get(ResourceKind.Industry);
            var starter = run.Timeline.Fleets[0].Id;

            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry - 10));
            Assert.That(run.Timeline.Fleets[0].ShipCount, Is.EqualTo(3));

            AdvanceTicks(stack.Loop, stack.Clock, Ticks(10) - 1);
            Assert.That(run.Timeline.Fleets[0].ShipCount, Is.EqualTo(3));

            var finishedOrderStillQueued = false;
            using (stack.Events.Subscribe<ShipConstructionCompletedEvent>(completed =>
            {
                Assert.That(run.Timeline.TryGetColony(HomePlanet(run), out var building), Is.True);
                for (var i = 0; i < building.BuildQueue.Count; i++)
                {
                    var order = building.BuildQueue[i];
                    if (order.ClassId == completed.ClassId && order.SecondsRemaining <= 0f)
                    {
                        finishedOrderStillQueued = true;
                    }
                }
            }))
            {
                AdvanceTicks(stack.Loop, stack.Clock, 1);
            }

            Assert.That(finishedOrderStillQueued, Is.False);

            Assert.That(run.Timeline.Fleets.TryGetFleet(starter, out var fleet), Is.True);
            Assert.That(fleet.ShipCount, Is.EqualTo(4));
            Assert.That(fleet.CountOf(Screen), Is.EqualTo(4));
            Assert.That(run.Timeline.TryGetColony(HomePlanet(run), out var colony), Is.True);
            Assert.That(colony.BuildQueue, Is.Empty);
        }

        [Test]
        public void Cancel_RefundsTheCost()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 91);
            Unlock(stack, run, "basic-fleet");
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
            var industry = run.Timeline.Resources.Get(ResourceKind.Industry);

            Assert.That(stack.ShipProductionService.Cancel(run, HomePlanet(run), 0).Success, Is.True);

            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry + 10));
            Assert.That(run.Timeline.TryGetColony(HomePlanet(run), out var colony), Is.True);
            Assert.That(colony.BuildQueue, Is.Empty);
        }

        [Test]
        public void Queue_RejectsALockedClassUntilTheResearchIsCompleted()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 92);
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            var industry = run.Timeline.Resources.Get(ResourceKind.Industry);

            var locked = stack.ShipProductionService.Queue(run, HomePlanet(run), Screen);

            Assert.That(locked.Success, Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry));

            Unlock(stack, run, "basic-fleet");

            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
        }

        [Test]
        public void Queue_RejectsMissingShipyardOutpostFullQueueAndShortStockpile()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 93);
            Unlock(stack, run, "basic-fleet");
            var before = run.Timeline.Resources.Get(ResourceKind.Industry);

            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(before));

            var outpost = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));
            Assert.That(run.Timeline.TryAddColony(new ColonyState(outpost, ColonyPresence.Outpost)), Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, outpost, Screen).Success, Is.False);

            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            for (var i = 0; i < ColonyState.kMaxBuildOrders; i++)
            {
                Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
            }

            var full = run.Timeline.Resources.Get(ResourceKind.Industry);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(full));

            run.Timeline.Resources.TrySpend(new[] { new ResourceAmount(ResourceKind.Industry, full) });
            Assert.That(run.Timeline.TryGetColony(HomePlanet(run), out var colony), Is.True);
            colony.TryTakeBuild(0, out _);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(0));
        }

        [Test]
        public void Abandon_DropsTheQueueWithoutARefund()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 101);
            Unlock(stack, run, "basic-fleet");
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
            var industry = run.Timeline.Resources.Get(ResourceKind.Industry);

            Assert.That(stack.SettlementService.Abandon(run, HomePlanet(run)).Success, Is.True);

            Assert.That(run.Timeline.TryGetColony(HomePlanet(run), out _), Is.False);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry));
        }

        [Test]
        public void UnplaceableShip_IsDroppedAndRefundedOnce()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 102);
            Unlock(stack, run, "basic-fleet");
            var planet = ContentId.From("missing-planet");
            var colony = new ColonyState(planet, ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(ShipProductionServiceId(), 1));
            Assert.That(run.Timeline.TryAddColony(colony), Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, planet, Screen).Success, Is.True);
            var paid = run.Timeline.Resources.Get(ResourceKind.Industry);

            LogAssert.Expect(
                LogType.Warning,
                "ShipProductionSystem dropped a finished starter-screen on 'missing-planet' and refunded it because it could not be placed.");
            AdvanceTicks(stack.Loop, stack.Clock, Ticks(10));

            Assert.That(run.Timeline.TryGetColony(planet, out var after), Is.True);
            Assert.That(after.BuildQueue, Is.Empty);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(paid + 10));

            AdvanceTicks(stack.Loop, stack.Clock, Ticks(10));
        }

        [Test]
        public void RemoveShips_RejectsATravelingFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 103);
            var fleetId = run.Timeline.Fleets[0].Id;
            stack.Clock.SetPaused(false);
            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, RunTestFixture.FirstNeighbor(run)).Success,
                Is.True);

            Assert.That(run.Timeline.Fleets.TryRemoveShips(fleetId, Screen, 1), Is.False);
            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.ShipCount, Is.EqualTo(3));
        }

        [Test]
        public void FinishedShip_JoinsAnIdleFleetAndSkipsAMovingOne()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 94);
            Unlock(stack, run, "basic-fleet");
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            var starter = run.Timeline.Fleets[0].Id;
            Assert.That(run.Timeline.Fleets.TrySetFleetLocation(starter, RunTestFixture.FirstNeighbor(run)), Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);

            AdvanceTicks(stack.Loop, stack.Clock, Ticks(10));

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(run.Timeline.Fleets.TryGetFleet(starter, out var away), Is.True);
            Assert.That(away.ShipCount, Is.EqualTo(3));
            Assert.That(away.LocationSystemId, Is.EqualTo(RunTestFixture.FirstNeighbor(run)));
        }

        [Test]
        public void Merge_RequiresTheSameNodeAndKeepsTheShips()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 95);
            var home = RunTestFixture.HomeSystem(run);
            var target = run.Timeline.Fleets[0].Id;
            Assert.That(run.Timeline.Fleets.TryAddFleet(Line, 2, home, out var source), Is.True);
            Assert.That(run.Timeline.Fleets.TrySetFleetLocation(source, RunTestFixture.FirstNeighbor(run)), Is.True);

            Assert.That(stack.FleetService.Merge(run, target, source).Success, Is.False);

            Assert.That(run.Timeline.Fleets.TrySetFleetLocation(source, home), Is.True);
            Assert.That(stack.FleetService.Merge(run, target, source).Success, Is.True);

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.Fleets.TryGetFleet(target, out var merged), Is.True);
            Assert.That(merged.ShipCount, Is.EqualTo(5));
            Assert.That(merged.CountOf(Screen), Is.EqualTo(3));
            Assert.That(merged.CountOf(Line), Is.EqualTo(2));
            Assert.That(run.Timeline.Fleets.TryGetFleet(source, out _), Is.False);
        }

        [Test]
        public void Split_KeepsTheTotalAndRejectsTheWholeFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 96);
            var fleetId = run.Timeline.Fleets[0].Id;

            Assert.That(stack.FleetService.Split(run, fleetId, Screen, 3, out _).Success, Is.False);
            Assert.That(stack.FleetService.Split(run, fleetId, Screen, 4, out _).Success, Is.False);
            Assert.That(stack.FleetService.Split(run, fleetId, Screen, 1, out var created).Success, Is.True);

            Assert.That(run.Timeline.Fleets.TryGetFleet(fleetId, out var source), Is.True);
            Assert.That(run.Timeline.Fleets.TryGetFleet(created, out var split), Is.True);
            Assert.That(source.ShipCount + split.ShipCount, Is.EqualTo(3));
            Assert.That(split.CountOf(Screen), Is.EqualTo(1));
        }

        [Test]
        public void Travel_UsesTheSlowestClassAndTheResearchMultiplier()
        {
            var slow = Duration(97, withTravelResearch: false, lineShip: true);
            var fast = Duration(98, withTravelResearch: false, lineShip: false);
            var boosted = Duration(99, withTravelResearch: true, lineShip: false);

            Assert.That(slow, Is.EqualTo(6f).Within(0.001f));
            Assert.That(fast, Is.EqualTo(3f).Within(0.001f));
            Assert.That(boosted, Is.EqualTo(3f / 1.25f).Within(0.001f));
        }

        [Test]
        public void TimelineReset_ClearsFleetsAndQueuesAndKeepsResearch()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = HomeRun(stack, 100);
            Unlock(stack, run, "basic-fleet");
            Unlock(stack, run, "basic-travel");
            Assert.That(stack.BuildingService.Place(run, HomePlanet(run), ShipProductionServiceId()).Success, Is.True);
            Assert.That(stack.ShipProductionService.Queue(run, HomePlanet(run), Screen).Success, Is.True);
            Assert.That(run.Timeline.Fleets.TryAddFleet(Line, 1, RunTestFixture.HomeSystem(run), out _), Is.True);

            var timeline = stack.Manager.RecreateTimeline(1);

            Assert.That(timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(timeline.Fleets[0].ShipCount, Is.EqualTo(3));
            Assert.That(timeline.TryGetColony(HomePlanet(run), out var colony), Is.True);
            Assert.That(colony.BuildQueue, Is.Empty);
            Assert.That(run.Archive.Research.Contains(ContentId.From("basic-travel")), Is.True);
            Assert.That(stack.Effects.TravelSpeedMultiplier(), Is.EqualTo(1.25f).Within(0.001f));
        }

        private static float Duration(int seed, bool withTravelResearch, bool lineShip)
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(GalaxyGenerator.ScenarioId, RunSeed.FromValue(seed));
            if (withTravelResearch)
            {
                Unlock(stack, run, "basic-travel");
            }

            var fleetId = run.Timeline.Fleets[0].Id;
            if (lineShip)
            {
                Assert.That(run.Timeline.Fleets.TryAddShips(fleetId, Line, 1), Is.True);
            }

            stack.Clock.SetPaused(false);
            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, RunTestFixture.FirstNeighbor(run)).Success,
                Is.True);
            var travel = run.Timeline.Fleets.ActiveTravels[0];
            return (float)(travel.ArriveTime.Seconds - travel.DepartTime.Seconds);
        }

        private static RunState HomeRun(OverworldTestContext stack, int seed)
        {
            return stack.Manager.CreateRun(GalaxyGenerator.ScenarioId, RunSeed.FromValue(seed));
        }

        private static void Unlock(OverworldTestContext stack, RunState run, string researchId)
        {
            var id = ContentId.From(researchId);
            Assert.That(run.Archive.Research.TryAdd(id), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
        }

        private static ContentId ShipProductionServiceId()
        {
            return ContentId.From("shipyard");
        }

        private static int Ticks(float seconds)
        {
            return (int)System.Math.Round(seconds / SimulationLoop.kTickIntervalSeconds);
        }

        private static void AdvanceTicks(SimulationLoop loop, SimulationClock clock, int ticks)
        {
            clock.SetPaused(false);
            for (var i = 0; i < ticks; i++)
            {
                loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }
        }

        private static ContentId HomePlanet(RunState run) => RunTestFixture.HomePlanet(run);

        private static readonly ContentId Screen = ContentId.From("starter-screen");

        private static readonly ContentId Line = ContentId.From("line-ship");
    }
}
