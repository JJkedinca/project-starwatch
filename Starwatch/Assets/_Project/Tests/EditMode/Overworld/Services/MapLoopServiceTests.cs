using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Services;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Services
{
    /// <summary>
    /// EditMode coverage for explore / scan / travel / visibility loop.
    /// </summary>
    public sealed class MapLoopServiceTests
    {
        [Test]
        public void GeneratedGalaxyGraph_IsConnected()
        {
            var run = RunTestFixture.CreateStandardRun(RunTestFixture.CreateRunManager(), 1);

            Assert.That(run.Universe.MapGraph.IsFullyConnected(), Is.True);
            Assert.That(run.Universe.MapGraph.NodeCount, Is.GreaterThanOrEqualTo(12));
        }

        [Test]
        public void Explore_RequiresFleetAtNode_AndDoesNotScanPlanets()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 2);
            var forge = RunTestFixture.FirstNeighbor(run);
            var planet = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));

            var blocked = stack.ExplorationService.Explore(run, ObserverId.Player, forge);

            Assert.That(blocked.Success, Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);
            var result = stack.ExplorationService.Explore(run, ObserverId.Player, forge);

            Assert.That(result.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            Assert.That(stack.Journal.Records, Has.Count.EqualTo(1));
            Assert.That(
                stack.VisibilityService.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, forge),
                Is.EqualTo(MapNodeVisibility.Present));
        }

        [Test]
        public void Explore_RejectsUnreachableNode()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 2);
            var frontier = ContentId.From("frontier-system");

            var result = stack.ExplorationService.Explore(run, ObserverId.Player, frontier);

            Assert.That(result.Success, Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(frontier), Is.False);
        }

        [Test]
        public void ScanPlanet_RequiresExploredSystemThenRecordsKnowledge()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 3);
            var planet = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));

            var blocked = stack.SensorService.ScanPlanet(run, ObserverId.Player, planet);
            Assert.That(blocked.Success, Is.False);

            run.Timeline.Fleets.TrySetFleetLocation(
                run.Timeline.Fleets[0].Id,
                RunTestFixture.FirstNeighbor(run));
            stack.ExplorationService.Explore(run, ObserverId.Player, RunTestFixture.FirstNeighbor(run));
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            var scanned = stack.SensorService.ScanPlanet(run, ObserverId.Player, planet);

            Assert.That(scanned.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.True);
            Assert.That(
                stack.VisibilityService.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    RunTestFixture.FirstNeighbor(run),
                    planet),
                Is.EqualTo(PlanetVisibility.Scanned));
        }

        [Test]
        public void ScanPlanet_RejectsPlanetNotOnActiveUniverseGraph()
        {
            var objectDatabase = RunTestFixture.CreateObjectDatabase();
            var foreignPlanet = ContentId.From("foreign-world");
            var foreign = ScriptableObject.CreateInstance<ScenarioDefinition>();
            foreign.Configure(
                "foreign-scenario",
                StartingEmpireDefinition.CreateDefault(),
                System.Array.Empty<MapNodeDefinition>(),
                System.Array.Empty<MapRouteDefinition>(),
                new[]
                {
                    new PlanetDefinition(
                        foreignPlanet,
                        RunTestFixture.HomeSystem(run),
                        "Foreign World",
                        "Registered elsewhere."),
                });
            objectDatabase.RegisterScenario(foreign);

            var stack = RunTestFixture.CreateOverworldStack();
            // Rebuild sensor against the database that includes the foreign planet.
            var sensorService = new SensorService(
                new ArchiveService(),
                objectDatabase,
                stack.Events,
                stack.Clock);
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 3);

            var result = sensorService.ScanPlanet(run, ObserverId.Player, foreignPlanet);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(foreignPlanet), Is.False);
        }

        [Test]
        public void Travel_ToUnexploredNeighbor_ArrivesAndExploresWithoutScanning()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 4);
            var fleetId = run.Timeline.Fleets[0].Id;
            var forge = RunTestFixture.FirstNeighbor(run);
            var planet = RunTestFixture.FirstPlanetAt(run, RunTestFixture.FirstNeighbor(run));

            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.False);
            stack.Clock.SetPaused(false);
            var started = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge);
            Assert.That(started.Success, Is.True);
            Assert.That(run.Timeline.Fleets.IsFleetTraveling(fleetId), Is.True);
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(RunTestFixture.HomeSystem(run)));

            // 3s travel at 30Hz; pump past arrival so SimulationLoop resolves TravelSystem.
            var ticks = 0;
            while (run.Timeline.Fleets.IsFleetTraveling(fleetId) && ticks < 200)
            {
                stack.Loop.Pump(1f / 30f);
                ticks++;
            }

            Assert.That(run.Timeline.Fleets.IsFleetTraveling(fleetId), Is.False);
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(forge));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            Assert.That(
                stack.Journal.Records[stack.Journal.Records.Count - 2].EventType,
                Is.EqualTo(GameEventType.FleetArrived));
            Assert.That(
                stack.Journal.Records[stack.Journal.Records.Count - 1].EventType,
                Is.EqualTo(GameEventType.NodeExplored));
            Assert.That(
                stack.VisibilityService.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, forge),
                Is.EqualTo(MapNodeVisibility.Present));
        }

        [Test]
        public void Travel_RejectsNonAdjacentDestination()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 5);
            var fleetId = run.Timeline.Fleets[0].Id;
            var frontier = ContentId.From("frontier-system");

            var result = stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, frontier);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ProjectStarwatch.Core.Commands.CommandErrorCode.InvalidRoute));
        }
    }
}
