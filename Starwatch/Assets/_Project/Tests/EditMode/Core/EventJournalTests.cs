using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Simulation;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for structured event journal records.
    /// </summary>
    public sealed class EventJournalTests
    {
        [Test]
        public void Record_StoresStructuredEvent()
        {
            var journal = new EventJournal();

            journal.Record(
                GameEventType.NodeExplored,
                new GameTime(12d),
                new EntityId(3),
                ContentId.From("node-a"));

            Assert.That(journal.Records, Has.Count.EqualTo(1));
            Assert.That(journal.Records[0].EventType, Is.EqualTo(GameEventType.NodeExplored));
            Assert.That(journal.Records[0].SimulationTime.Seconds, Is.EqualTo(12d));
            Assert.That(journal.Records[0].SourceId, Is.EqualTo(new EntityId(3)));
            Assert.That(journal.Records[0].PayloadId, Is.EqualTo(ContentId.From("node-a")));
        }

        [Test]
        public void Record_IgnoresNoneEventType()
        {
            var journal = new EventJournal();

            journal.Record(GameEventType.None, GameTime.Zero, EntityId.None, ContentId.From("x"));

            Assert.That(journal.Records, Is.Empty);
        }
    }
}
