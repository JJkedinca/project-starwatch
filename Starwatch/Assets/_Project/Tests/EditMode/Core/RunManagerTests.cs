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
        public void CreateRun_CreatesScenarioTimelineArchiveSeedAndStarterFleet()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            var seed = RunSeed.FromValue(42);
            var empire = new StartingEmpireDefinition(
                ContentId.From("home-a"),
                ContentId.From("screen"),
                starterShipCount: 2);

            var run = manager.CreateRun("authored-vertical-slice", seed, empire);

            Assert.That(run, Is.Not.Null);
            Assert.That(manager.HasActiveRun, Is.True);
            Assert.That(run.Seed.Value, Is.EqualTo(42));
            Assert.That(run.Scenario.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(run.Scenario.StartingEmpire.HomeSystemId, Is.EqualTo(ContentId.From("home-a")));
            Assert.That(run.Timeline, Is.Not.Null);
            Assert.That(run.Archive, Is.Not.Null);
            Assert.That(run.Random, Is.Not.Null);
            Assert.That(run.RewindIndex, Is.EqualTo(0));
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.Fleets[0].ShipCount, Is.EqualTo(2));
            Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(ContentId.From("home-a")));
        }

        [Test]
        public void CreateRun_RejectsMissingScenarioId()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            LogAssert.Expect(LogType.Error, "RunManager.CreateRun requires a scenario id.");

            var run = manager.CreateRun(" ", RunSeed.FromValue(1));

            Assert.That(run, Is.Null);
            Assert.That(manager.HasActiveRun, Is.False);
        }

        [Test]
        public void CreateRun_SkipsStarterFleetWhenHomeSystemMissing()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            var empire = new StartingEmpireDefinition(
                ContentId.From(" "),
                ContentId.From("screen"),
                starterShipCount: 2);

            var run = manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(7), empire);

            Assert.That(run, Is.Not.Null);
            Assert.That(empire.CanPlaceStarterFleet, Is.False);
            Assert.That(run.Timeline.Fleets, Is.Empty);
            Assert.That(run.Scenario.SystemIds, Is.Empty);
        }

        [Test]
        public void CreateRun_ClearsJournalFromPreviousRun()
        {
            var journal = new EventJournal();
            var manager = new RunManager(new TimelineFactory(), journal);
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
            var manager = new RunManager(new TimelineFactory(), journal);
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
            var manager = new RunManager(new TimelineFactory(), journal);
            journal.Record(
                GameEventType.NodeExplored,
                GameTime.Zero,
                new EntityId(1),
                ContentId.From("keep-me"));
            LogAssert.Expect(LogType.Error, "RunManager.CreateRun requires a scenario id.");

            manager.CreateRun(" ", RunSeed.FromValue(1));

            Assert.That(journal.Records, Has.Count.EqualTo(1));
        }
    }
}

