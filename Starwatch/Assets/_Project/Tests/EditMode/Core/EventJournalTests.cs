using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for structured event journal records.
    /// </summary>
    public sealed class EventJournalTests
    {
        [Test]
        public void Record_StoresTheDomainEvent()
        {
            var journal = new EventJournal();
            var subjectId = ContentId.From("node-a");
            var domainEvent = new MapKnowledgeChangedEvent(
                new GameTime(12d),
                ObserverId.Player,
                subjectId,
                MapKnowledgeKind.NodeExplored);

            journal.Record(domainEvent);

            Assert.That(journal.Records, Has.Count.EqualTo(1));
            Assert.That(journal.Records[0].EventType, Is.EqualTo(GameEventType.NodeExplored));
            Assert.That(journal.Records[0].SimulationTime.Seconds, Is.EqualTo(12d));
            Assert.That(journal.Records[0].TryGet<MapKnowledgeChangedEvent>(out var stored), Is.True);
            Assert.That(stored.ObserverId, Is.EqualTo(ObserverId.Player));
            Assert.That(stored.SubjectId, Is.EqualTo(subjectId));
            Assert.That(stored.Kind, Is.EqualTo(MapKnowledgeKind.NodeExplored));
        }

        [Test]
        public void Record_IgnoresNoneEventType()
        {
            var journal = new EventJournal();

            journal.Record(new NoneDomainEvent());

            Assert.That(journal.Records, Is.Empty);
        }

        private readonly struct NoneDomainEvent : IDomainEvent
        {
            public GameEventType EventType => GameEventType.None;

            public GameTime Time => GameTime.Zero;
        }
    }
}
