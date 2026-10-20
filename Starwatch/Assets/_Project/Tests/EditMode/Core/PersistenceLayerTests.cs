using System;
using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for universe / timeline / archive isolation.
    /// </summary>
    public sealed class PersistenceLayerTests
    {
        [Test]
        public void MutatingTimeline_DoesNotChangeUniverseOrArchive()
        {
            var run = CreateRun();
            var universeNodeCount = run.Universe.NodeIds.Count;
            var archiveExploreCount = run.Archive.PlayerMapKnowledge.ExploredNodeIds.Count;

            var result = TimelineMutationCommands.AddFleet(
                run.Timeline,
                ContentId.From("extra-fleet"),
                shipCount: 1,
                run.Universe.StartingEmpire.HomeSystemId);

            Assert.That(result.Success, Is.True);
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(run.Universe.NodeIds, Has.Count.EqualTo(universeNodeCount));
            Assert.That(run.Archive.PlayerMapKnowledge.ExploredNodeIds, Has.Count.EqualTo(archiveExploreCount));
        }

        [Test]
        public void MutatingArchive_DoesNotChangeTimelineOrUniverse()
        {
            var run = CreateRun();
            var fleetCount = run.Timeline.Fleets.Count;
            var nodeCount = run.Universe.NodeIds.Count;

            var result = TimelineMutationCommands.RecordExploredNode(
                run.Archive,
                ContentId.From("node-b"));

            Assert.That(result.Success, Is.True);
            Assert.That(run.Archive.PlayerMapKnowledge.ExploredNodeIds, Has.Count.EqualTo(2));
            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(fleetCount));
            Assert.That(run.Universe.NodeIds, Has.Count.EqualTo(nodeCount));
        }

        [Test]
        public void RecreateTimeline_DoesNotAliasCollectionsWithPreviousTimelineOrUniverse()
        {
            var manager = RunTestFixture.CreateRunManager();
            var run = manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(9));

            var oldTimeline = run.Timeline;
            var oldFleets = oldTimeline.Fleets;
            TimelineMutationCommands.AddFleet(
                oldTimeline,
                ContentId.From("old-only"),
                shipCount: 1,
                run.Universe.StartingEmpire.HomeSystemId);

            var firstStarterId = oldTimeline.Fleets[0].Id;
            var addedFleetId = oldTimeline.Fleets[1].Id;
            var newTimeline = manager.RecreateTimeline(rewindIndex: 1);

            Assert.That(newTimeline, Is.Not.SameAs(oldTimeline));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(oldFleets));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(run.Universe.NodeIds));
            Assert.That(newTimeline.Fleets, Is.Not.SameAs(run.Archive.PlayerMapKnowledge.ExploredNodeIds));
            Assert.That(run.RewindIndex, Is.EqualTo(1));
            Assert.That(newTimeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(oldTimeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(run.Universe.NodeIds.Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(run.Timeline.EntityIds, Is.SameAs(run.EntityIds));
            Assert.That(newTimeline.Fleets[0].Id.Value, Is.GreaterThan(addedFleetId.Value));
            Assert.That(newTimeline.Fleets[0].Id, Is.Not.EqualTo(firstStarterId));
        }

        [Test]
        public void RecreateTimeline_RejectsNegativeRewindIndex()
        {
            var manager = RunTestFixture.CreateRunManager();
            var run = manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(3));
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

            var added = timeline.Fleets.TryAddFleet(
                ContentId.From("screen"),
                shipCount: 1,
                ContentId.From(" "),
                out var id);

            Assert.That(added, Is.False);
            Assert.That(id.IsNone, Is.True);
            Assert.That(timeline.Fleets, Is.Empty);
        }

        [Test]
        public void Fleet_RejectsInvalidConstruction()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ShipStack(ContentId.From("screen"), 0));

            Assert.Throws<ArgumentException>(() =>
                new FleetComposition(
                    new EntityId(1),
                    ContentId.From(" "),
                    new[] { new ShipStack(ContentId.From("screen"), 1) }));
        }

        [Test]
        public void TryAddExploredNode_RejectsEmptyId()
        {
            var archive = new RunArchive();

            Assert.That(archive.TryAddExploredNode(ContentId.From(" ")), Is.False);
            Assert.That(archive.PlayerMapKnowledge.ExploredNodeIds, Is.Empty);
        }

        [Test]
        public void TryAddExploredNode_ReturnsFalseForDuplicates()
        {
            var archive = new RunArchive();
            var node = ContentId.From("home-system");

            Assert.That(archive.TryAddExploredNode(node), Is.True);
            Assert.That(archive.TryAddExploredNode(node), Is.False);
            Assert.That(archive.PlayerMapKnowledge.ExploredNodeIds, Has.Count.EqualTo(1));
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
            return RunTestFixture.CreateRunManager().CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(1));
        }
    }
}
