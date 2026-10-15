using EndlessHeros.Core.Commands;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Overworld.Services
{
    /// <summary>
    /// EditMode coverage for explore / scan / travel / visibility loop.
    /// </summary>
    public sealed class MapLoopServiceTests
    {
        [Test]
        public void VerticalSliceGraph_IsConnected()
        {
            var run = RunTestFixture.CreateRunManager().CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(1));

            Assert.That(run.Universe.MapGraph.IsFullyConnected(), Is.True);
            Assert.That(run.Universe.MapGraph.NodeCount, Is.EqualTo(7));
            Assert.That(run.Universe.MapGraph.Routes.Count, Is.EqualTo(7));
        }

        [Test]
        public void Explore_RequiresFleetAtNode_AndDoesNotScanPlanets()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(2));
            var forge = ContentId.From("forge-system");
            var planet = ContentId.From("forge-world");

            var blocked = stack.Exploration.Explore(run, ObserverId.Player, forge);

            Assert.That(blocked.Success, Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.False);

            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);
            var result = stack.Exploration.Explore(run, ObserverId.Player, forge);

            Assert.That(result.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            Assert.That(stack.Journal.Records, Has.Count.EqualTo(1));
            Assert.That(
                stack.Visibility.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, forge),
                Is.EqualTo(MapNodeVisibility.Present));
        }

        [Test]
        public void Explore_RejectsUnreachableNode()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(2));
            var frontier = ContentId.From("frontier-system");

            var result = stack.Exploration.Explore(run, ObserverId.Player, frontier);

            Assert.That(result.Success, Is.False);
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(frontier), Is.False);
        }

        [Test]
        public void ScanPlanet_RequiresExploredSystemThenRecordsKnowledge()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(3));
            var planet = ContentId.From("forge-world");

            var blocked = stack.Sensor.ScanPlanet(run, ObserverId.Player, planet);
            Assert.That(blocked.Success, Is.False);

            run.Timeline.TrySetFleetLocation(
                run.Timeline.Fleets[0].Id,
                ContentId.From("forge-system"));
            stack.Exploration.Explore(run, ObserverId.Player, ContentId.From("forge-system"));
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            var scanned = stack.Sensor.ScanPlanet(run, ObserverId.Player, planet);

            Assert.That(scanned.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.True);
            Assert.That(
                stack.Visibility.GetPlanetVisibility(
                    run.Archive,
                    ObserverId.Player,
                    ContentId.From("forge-system"),
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
                        ContentId.From("home-system"),
                        "Foreign World",
                        "Registered elsewhere."),
                });
            objectDatabase.RegisterScenario(foreign);

            var stack = RunTestFixture.CreateOverworldStack();
            // Rebuild sensor against the database that includes the foreign planet.
            var sensor = new SensorService(
                new ArchiveService(),
                objectDatabase,
                stack.Journal,
                stack.Clock);
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(3));

            var result = sensor.ScanPlanet(run, ObserverId.Player, foreignPlanet);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(foreignPlanet), Is.False);
        }

        [Test]
        public void Travel_ToUnexploredNeighbor_ArrivesAndExploresWithoutScanning()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(4));
            var fleetId = run.Timeline.Fleets[0].Id;
            var forge = ContentId.From("forge-system");
            var planet = ContentId.From("forge-world");

            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.False);
            stack.Clock.SetPaused(false);
            var started = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, forge);
            Assert.That(started.Success, Is.True);
            Assert.That(run.Timeline.IsFleetTraveling(fleetId), Is.True);
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(ContentId.From("home-system")));

            // 3s travel at 30Hz; pump past arrival so SimulationLoop resolves TravelSystem.
            var ticks = 0;
            while (run.Timeline.IsFleetTraveling(fleetId) && ticks < 200)
            {
                stack.Loop.Pump(1f / 30f);
                ticks++;
            }

            Assert.That(run.Timeline.IsFleetTraveling(fleetId), Is.False);
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(forge));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            Assert.That(
                stack.Visibility.GetNodeVisibility(run.Archive, run.Timeline, ObserverId.Player, forge),
                Is.EqualTo(MapNodeVisibility.Present));
        }

        [Test]
        public void Travel_RejectsNonAdjacentDestination()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(5));
            var fleetId = run.Timeline.Fleets[0].Id;
            var frontier = ContentId.From("frontier-system");

            var result = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, frontier);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(EndlessHeros.Core.Commands.CommandErrorCode.InvalidRoute));
        }
    }
}
