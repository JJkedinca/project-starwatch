using EndlessHeros.Core.Data;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EntityId = EndlessHeros.Core.Identifiers.EntityId;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for RunManager run-creation contracts.
    /// </summary>
    public sealed class RunManagerTests
    {
        [Test]
        public void CreateRun_CreatesUniverseTimelineArchiveSeedAndStarterFleet()
        {
            var manager = RunTestFixture.CreateRunManager();
            var seed = RunSeed.FromValue(42);

            var run = manager.CreateRun("authored-vertical-slice", seed);

            Assert.That(run, Is.Not.Null);
            Assert.That(manager.HasActiveRun, Is.True);
            Assert.That(run.Seed.Value, Is.EqualTo(42));
            Assert.That(run.Universe.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(run.Universe.StartingEmpire.HomeSystemId, Is.EqualTo(ContentId.From("home-system")));
            Assert.That(run.Universe.MapGraph.IsFullyConnected(), Is.True);
            Assert.That(run.Universe.MapGraph.NodeCount, Is.EqualTo(7));
            Assert.That(run.Timeline, Is.Not.Null);
            Assert.That(run.Archive, Is.Not.Null);
            Assert.That(run.Random, Is.Not.Null);
            Assert.That(run.RewindIndex, Is.EqualTo(0));
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.Fleets[0].ShipCount, Is.EqualTo(3));
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(ContentId.From("home-system")));
            Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(ContentId.From("home-system")), Is.True);
        }

        [Test]
        public void CreateRun_RejectsMissingScenarioId()
        {
            var manager = RunTestFixture.CreateRunManager();
            LogAssert.Expect(LogType.Error, "RunManager.CreateRun requires a scenario id.");

            var run = manager.CreateRun(" ", RunSeed.FromValue(1));

            Assert.That(run, Is.Null);
            Assert.That(manager.HasActiveRun, Is.False);
        }

        [Test]
        public void CreateRun_RejectsStartingHomeMissingFromMap()
        {
            var manager = RunTestFixture.CreateRunManager();
            var empire = new StartingEmpireDefinition(
                ContentId.From("missing-home"),
                ContentId.From("screen"),
                starterShipCount: 2);
            LogAssert.Expect(
                LogType.Error,
                "UniverseLoader.TryLoad starting home must be a MapNodeKind.System node on the map.");

            var run = manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(7), empire);

            Assert.That(run, Is.Null);
        }

        [Test]
        public void CreateRun_RejectsStartingHomeThatIsNonSystemNode()
        {
            var manager = RunTestFixture.CreateRunManager();
            var empire = new StartingEmpireDefinition(
                ContentId.From("asteroid-belt"),
                ContentId.From("screen"),
                starterShipCount: 2);
            LogAssert.Expect(
                LogType.Error,
                "UniverseLoader.TryLoad starting home must be a MapNodeKind.System node on the map.");

            var run = manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(8), empire);

            Assert.That(run, Is.Null);
        }

        [Test]
        public void CreateRun_RejectsScenarioWithNegativeRouteDuration()
        {
            var objectDatabase = RunTestFixture.CreateObjectDatabase();
            var home = ContentId.From("bad-home");
            var other = ContentId.From("bad-other");
            var definition = ScriptableObject.CreateInstance<ScenarioDefinition>();
            definition.Configure(
                "bad-duration-scenario",
                new StartingEmpireDefinition(home, ContentId.From("screen"), starterShipCount: 1),
                new[]
                {
                    new MapNodeDefinition(home, MapNodeKind.System, Vector2.zero),
                    new MapNodeDefinition(other, MapNodeKind.System, Vector2.one),
                },
                new[] { new MapRouteDefinition(home, other, -2f) },
                System.Array.Empty<PlanetDefinition>());
            objectDatabase.RegisterScenario(definition);
            var manager = RunTestFixture.CreateRunManager(objectDatabase);
            LogAssert.Expect(
                LogType.Error,
                "UniverseLoader.TryLoad map route at index 0 has a negative or invalid duration.");

            var run = manager.CreateRun("bad-duration-scenario", RunSeed.FromValue(1));

            Assert.That(run, Is.Null);
        }

        [Test]
        public void CreateRun_ClearsJournalFromPreviousRun()
        {
            var journal = new EventJournal();
            var manager = RunTestFixture.CreateRunManager(journal: journal);
            journal.Record(
                GameEventType.NodeExplored,
                GameTime.Zero,
                new EntityId(1),
                ContentId.From("old-node"));

            manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(1));

            Assert.That(journal.Records, Is.Empty);
        }

        [Test]
        public void ClearRun_ClearsJournal()
        {
            var journal = new EventJournal();
            var manager = RunTestFixture.CreateRunManager(journal: journal);
            manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(1));
            journal.Record(
                GameEventType.NodeExplored,
                GameTime.Zero,
                new EntityId(1),
                ContentId.From("node-a"));

            manager.ClearRun();

            Assert.That(manager.HasActiveRun, Is.False);
            Assert.That(journal.Records, Is.Empty);
        }

        [Test]
        public void CreateRun_DoesNotClearJournalWhenRejected()
        {
            var journal = new EventJournal();
            var manager = RunTestFixture.CreateRunManager(journal: journal);
            journal.Record(
                GameEventType.NodeExplored,
                GameTime.Zero,
                new EntityId(1),
                ContentId.From("keep-me"));
            LogAssert.Expect(LogType.Error, "RunManager.CreateRun requires a scenario id.");

            manager.CreateRun(" ", RunSeed.FromValue(1));

            Assert.That(journal.Records, Has.Count.EqualTo(1));
        }

        [Test]
        public void CreateRun_DoesNotClearJournalWhenLoadFails()
        {
            var journal = new EventJournal();
            var manager = RunTestFixture.CreateRunManager(journal: journal);
            manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(1));
            journal.Record(
                GameEventType.NodeExplored,
                GameTime.Zero,
                new EntityId(1),
                ContentId.From("keep-me"));

            var empire = new StartingEmpireDefinition(
                ContentId.From("missing-home"),
                ContentId.From("screen"),
                starterShipCount: 2);
            LogAssert.Expect(
                LogType.Error,
                "UniverseLoader.TryLoad starting home must be a MapNodeKind.System node on the map.");

            var failed = manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(7), empire);

            Assert.That(failed, Is.Null);
            Assert.That(manager.HasActiveRun, Is.True);
            Assert.That(journal.Records, Has.Count.EqualTo(1));
            Assert.That(journal.Records[0].PayloadId, Is.EqualTo(ContentId.From("keep-me")));
        }
    }
}
