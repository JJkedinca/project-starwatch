using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for TimelineState travel invariants.
    /// </summary>
    public sealed class TimelineStateTests
    {
        [Test]
        public void TryStartTravel_RejectsWrongSourceLocation()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("home-system"),
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                ContentId.From("forge-system"),
                ContentId.From("choke-system"),
                GameTime.Zero,
                new GameTime(3d));

            Assert.That(timeline.TryStartTravel(travel), Is.False);
            Assert.That(timeline.ActiveTravels, Is.Empty);
        }

        [Test]
        public void TryStartTravel_RejectsArrivalBeforeDeparture()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("home-system"),
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                ContentId.From("home-system"),
                ContentId.From("forge-system"),
                new GameTime(5d),
                new GameTime(2d));

            Assert.That(timeline.TryStartTravel(travel), Is.False);
            Assert.That(timeline.ActiveTravels, Is.Empty);
        }

        [Test]
        public void TryStartTravel_AcceptsValidLeg()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("home-system"),
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                ContentId.From("home-system"),
                ContentId.From("forge-system"),
                GameTime.Zero,
                new GameTime(3d));

            Assert.That(timeline.TryStartTravel(travel), Is.True);
            Assert.That(timeline.ActiveTravels, Has.Count.EqualTo(1));
        }

        [Test]
        public void CompleteArrivals_PreservesInsertionOrderInBuffer()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("home-system"),
                    out var firstFleet),
                Is.True);
            Assert.That(
                timeline.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("forge-system"),
                    out var secondFleet),
                Is.True);

            var first = new ActiveTravel(
                firstFleet,
                ContentId.From("home-system"),
                ContentId.From("forge-system"),
                GameTime.Zero,
                new GameTime(1d));
            var second = new ActiveTravel(
                secondFleet,
                ContentId.From("forge-system"),
                ContentId.From("choke-system"),
                GameTime.Zero,
                new GameTime(1d));

            Assert.That(timeline.TryStartTravel(first), Is.True);
            Assert.That(timeline.TryStartTravel(second), Is.True);

            System.Collections.Generic.List<ActiveTravel> completed = new();
            var count = timeline.CompleteArrivals(new GameTime(1d), completed);

            Assert.That(count, Is.EqualTo(2));
            Assert.That(completed, Has.Count.EqualTo(2));
            Assert.That(completed[0].FleetId, Is.EqualTo(firstFleet));
            Assert.That(completed[1].FleetId, Is.EqualTo(secondFleet));
        }

        [Test]
        public void FleetIndex_FollowsLocationAndTravel()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            Assert.That(
                timeline.TryAddFleet(ContentId.From("screen"), shipCount: 1, home, out var first),
                Is.True);
            Assert.That(
                timeline.TryAddFleet(ContentId.From("screen"), shipCount: 1, home, out var second),
                Is.True);
            Assert.That(timeline.HasFleetAt(home), Is.True);
            Assert.That(timeline.HasTravelingFleetAt(home), Is.False);

            Assert.That(
                timeline.TryStartTravel(new ActiveTravel(
                    first,
                    home,
                    forge,
                    GameTime.Zero,
                    new GameTime(3d))),
                Is.True);
            Assert.That(timeline.HasTravelingFleetAt(home), Is.True);
            Assert.That(timeline.IsFleetTraveling(first), Is.True);
            Assert.That(timeline.IsFleetTraveling(second), Is.False);

            Assert.That(timeline.TrySetFleetLocation(second, forge), Is.True);
            Assert.That(timeline.HasFleetAt(home), Is.True);
            Assert.That(timeline.HasFleetAt(forge), Is.True);

            System.Collections.Generic.List<ActiveTravel> completed = new();
            timeline.CompleteArrivals(new GameTime(3d), completed);

            Assert.That(timeline.HasFleetAt(home), Is.False);
            Assert.That(timeline.HasFleetAt(forge), Is.True);
            Assert.That(timeline.HasTravelingFleetAt(home), Is.False);
            Assert.That(timeline.HasTravelingFleetAt(forge), Is.False);
            Assert.That(timeline.IsFleetTraveling(first), Is.False);
            Assert.That(timeline.TryGetFleet(first, out var moved), Is.True);
            Assert.That(moved.LocationSystemId, Is.EqualTo(forge));
        }
    }
}
