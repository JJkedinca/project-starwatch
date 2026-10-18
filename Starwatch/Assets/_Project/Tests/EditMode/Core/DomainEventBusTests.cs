using System.Collections.Generic;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for the domain event bus: journal plus subscribers.
    /// </summary>
    public sealed class DomainEventBusTests
    {
        [Test]
        public void Publish_JournalsAndNotifiesSubscribers()
        {
            var journal = new EventJournal();
            var bus = new DomainEventBus(journal);
            var received = new List<FleetTravelEvent>();
            bus.Subscribe<FleetTravelEvent>(received.Add);

            var fleetId = new EntityId(4);
            var fromNodeId = ContentId.From("home-system");
            var toNodeId = ContentId.From("forge-system");
            bus.Publish(new FleetTravelEvent(
                GameTime.Zero,
                fleetId,
                fromNodeId,
                toNodeId,
                FleetTravelKind.Arrived));

            Assert.That(journal.Records, Has.Count.EqualTo(1));
            Assert.That(journal.Records[0].EventType, Is.EqualTo(GameEventType.FleetArrived));
            Assert.That(journal.Records[0].TryGet<FleetTravelEvent>(out var stored), Is.True);
            Assert.That(stored.FleetId, Is.EqualTo(fleetId));
            Assert.That(stored.FromNodeId, Is.EqualTo(fromNodeId));
            Assert.That(stored.ToNodeId, Is.EqualTo(toNodeId));
            Assert.That(received, Has.Count.EqualTo(1));
            Assert.That(received[0].Kind, Is.EqualTo(FleetTravelKind.Arrived));
            Assert.That(received[0].ToNodeId, Is.EqualTo(toNodeId));
        }

        [Test]
        public void Publish_DoesNotNotifyAfterUnsubscribe()
        {
            var bus = new DomainEventBus(new EventJournal());
            var calls = 0;
            var subscription = bus.Subscribe<MapKnowledgeChangedEvent>(_ => calls++);

            bus.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.Player,
                ContentId.From("home-system"),
                MapKnowledgeKind.NodeExplored));
            subscription.Dispose();
            bus.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.Player,
                ContentId.From("forge-system"),
                MapKnowledgeKind.NodeExplored));

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Publish_LeavesOtherEventTypesAlone()
        {
            var bus = new DomainEventBus(new EventJournal());
            var travels = 0;
            bus.Subscribe<FleetTravelEvent>(_ => travels++);

            bus.Publish(new MapKnowledgeChangedEvent(
                GameTime.Zero,
                ObserverId.Player,
                ContentId.From("forge-world"),
                MapKnowledgeKind.PlanetScanned));

            Assert.That(travels, Is.EqualTo(0));
        }
    }
}
