using System;
using EndlessHeros.Core.Commands;
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
    /// EditMode coverage for scenario / timeline / archive isolation.
    /// </summary>
    public sealed class PersistenceLayerTests
    {
        [Test]
        public void MutatingTimeline_DoesNotChangeScenarioOrArchive()
        {
            var run = CreateRun();
            var scenarioSystemCount = run.Scenario.SystemIds.Count;
            var archiveExploreCount = run.Archive.ExploredNodeIds.Count;

            var result = TimelineMutationCommands.AddPlaceholderFleet(
                run.Timeline,
                ContentId.From("extra-fleet"),
                shipCount: 1,
                run.Scenario.StartingEmpire.HomeSystemId);

            Assert.That(result.Success, Is.True);
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(run.Scenario.SystemIds, Has.Count.EqualTo(scenarioSystemCount));
            Assert.That(run.Archive.ExploredNodeIds, Has.Count.EqualTo(archiveExploreCount));
        }

        [Test]
        public void MutatingArchive_DoesNotChangeTimelineOrScenario()
        {
            var run = CreateRun();
            var fleetCount = run.Timeline.Fleets.Count;
            var systemCount = run.Scenario.SystemIds.Count;

            var result = TimelineMutationCommands.RecordExploredNode(
                run.Archive,
                ContentId.From("node-b"));

            Assert.That(result.Success, Is.True);
            Assert.That(run.Archive.ExploredNodeIds, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(fleetCount));
            Assert.That(run.Scenario.SystemIds, Has.Count.EqualTo(systemCount));
        }

        [Test]
        public void RecreateTimeline_DoesNotAliasCollectionsWithPreviousTimelineOrScenario()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            var run = manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(9),
                StartingEmpireDefinition.CreateDefault());

            var oldTimeline = run.Timeline;
            var oldFleets = oldTimeline.Fleets;
            TimelineMutationCommands.AddPlaceholderFleet(
                oldTimeline,
                ContentId.From("old-only"),
                shipCount: 1,
                run.Scenario.StartingEmpire.HomeSystemId);

            var firstStarterId = oldTimeline.Fleets[0].Id;
            var addedFleetId = oldTimeline.Fleets[1].Id;
            var newTimeline = manager.RecreateTimeline(rewindIndex: 1);

            Assert.That(newTimeline, Is.Not.SameAs(oldTimeline));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(oldFleets));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(run.Scenario.SystemIds));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(run.Archive.ExploredNodeIds));
            Assert.That(run.RewindIndex, Is.EqualTo(1));
            Assert.That(newTimeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(oldTimeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(run.Scenario.SystemIds, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.EntityIds, Is.SameAs(run.EntityIds));
            Assert.That(newTimeline.Fleets[0].Id.Value, Is.GreaterThan(addedFleetId.Value));
            Assert.That(newTimeline.Fleets[0].Id, Is.Not.EqualTo(firstStarterId));
        }

        [Test]
        public void RecreateTimeline_RejectsNegativeRewindIndex()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            var run = manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(3),
                StartingEmpireDefinition.CreateDefault());
            var timelineBefore = run.Timeline;

            LogAssert.Expect(
                LogType.Error,
                "RunManager.RecreateTimeline requires a non-negative rewind index.");
            var result = manager.RecreateTimeline(rewindIndex: -1);

            Assert.That(result, Is.Null);
            Assert.That(run.RewindIndex, Is.EqualTo(0));
            Assert.That(run.Timeline, Is.SameAs(timelineBefore));
        }

        [Test]
        public void TryAddFleet_RejectsEmptyLocation()
        {
            var timeline = new TimelineState(new EntityIdFactory());

            var added = timeline.TryAddFleet(
                ContentId.From("screen"),
                shipCount: 1,
                ContentId.From(" "),
                out var id);

            Assert.That(added, Is.False);
            Assert.That(id.IsNone, Is.True);
            Assert.That(timeline.Fleets, Is.Empty);
        }

        [Test]
        public void FleetPlaceholderState_RejectsInvalidConstruction()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new FleetPlaceholderState(
                    new EntityId(1),
                    ContentId.From("screen"),
                    shipCount: 0,
                    ContentId.From("home")));

            Assert.Throws<ArgumentException>(() =>
                new FleetPlaceholderState(
                    new EntityId(1),
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From(" ")));
        }

        [Test]
        public void TryAddExploredNode_RejectsEmptyId()
        {
            var archive = new RunArchive();

            Assert.That(archive.TryAddExploredNode(ContentId.From(" ")), Is.False);
            Assert.That(archive.ExploredNodeIds, Is.Empty);
        }

        [Test]
        public void Clock_ResetsForRunAndAdvancesWhenResumed()
        {
            var clock = new SimulationClock();
            Assert.That(clock.IsPaused, Is.True);

            clock.SetPaused(false);
            clock.Advance(2f);
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(2d));

            clock.ResetForRun();
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(0d));
            Assert.That(clock.IsPaused, Is.True);
            Assert.That(clock.Advance(1f), Is.EqualTo(0f));

            clock.SetPaused(false);
            Assert.That(clock.Advance(0.25f), Is.EqualTo(0.25f));
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(0.25d));
        }

        private static RunState CreateRun()
        {
            var manager = new RunManager(new TimelineFactory(), new EventJournal());
            return manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(1),
                StartingEmpireDefinition.CreateDefault());
        }
    }
}
