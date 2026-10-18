using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for command rejection without partial mutation.
    /// </summary>
    public sealed class CommandResultTests
    {
        [Test]
        public void InvalidFleetCommand_DoesNotMutateTimeline()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            var before = timeline.Fleets.Count;

            var result = TimelineMutationCommands.AddPlaceholderFleet(
                timeline,
                ContentId.From("screen"),
                shipCount: 0,
                ContentId.From("home"));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.NotAllowed));
            Assert.That(timeline.Fleets, Has.Count.EqualTo(before));
        }

        [Test]
        public void InvalidArchiveCommand_DoesNotMutateArchive()
        {
            var archive = new RunArchive();

            var result = TimelineMutationCommands.RecordExploredNode(archive, ContentId.From(" "));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(CommandErrorCode.InvalidArgument));
            Assert.That(archive.PlayerMapKnowledge.ExploredNodeIds, Is.Empty);
        }
    }
}
