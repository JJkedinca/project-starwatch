using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for fleet ledger travel and ship-stack invariants.
    /// </summary>
    public sealed class TimelineStateTests
    {
        [Test]
        public void TryStartTravel_RejectsWrongSourceLocation()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(
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

            Assert.That(timeline.Fleets.TryStartTravel(travel), Is.False);
            Assert.That(timeline.Fleets.ActiveTravels, Is.Empty);
        }

        [Test]
        public void TryStartTravel_RejectsArrivalBeforeDeparture()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(
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

            Assert.That(timeline.Fleets.TryStartTravel(travel), Is.False);
            Assert.That(timeline.Fleets.ActiveTravels, Is.Empty);
        }

        [Test]
        public void TryStartTravel_AcceptsValidLeg()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(
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

            Assert.That(timeline.Fleets.TryStartTravel(travel), Is.True);
            Assert.That(timeline.Fleets.ActiveTravels, Has.Count.EqualTo(1));
        }

        [Test]
        public void CompleteArrivals_PreservesInsertionOrderInBuffer()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    ContentId.From("home-system"),
                    out var firstFleet),
                Is.True);
            Assert.That(
                timeline.Fleets.TryAddFleet(
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

            Assert.That(timeline.Fleets.TryStartTravel(first), Is.True);
            Assert.That(timeline.Fleets.TryStartTravel(second), Is.True);

            System.Collections.Generic.List<ActiveTravel> completed = new();
            var count = timeline.Fleets.CompleteArrivals(new GameTime(1d), completed);

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
                timeline.Fleets.TryAddFleet(ContentId.From("screen"), shipCount: 1, home, out var first),
                Is.True);
            Assert.That(
                timeline.Fleets.TryAddFleet(ContentId.From("screen"), shipCount: 1, home, out var second),
                Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(home), Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(home), Is.False);

            Assert.That(
                timeline.Fleets.TryStartTravel(new ActiveTravel(
                    first,
                    home,
                    forge,
                    GameTime.Zero,
                    new GameTime(3d))),
                Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(home), Is.True);
            Assert.That(timeline.Fleets.IsFleetTraveling(first), Is.True);
            Assert.That(timeline.Fleets.IsFleetTraveling(second), Is.False);

            Assert.That(timeline.Fleets.TrySetFleetLocation(second, forge), Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(home), Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(forge), Is.True);

            System.Collections.Generic.List<ActiveTravel> completed = new();
            timeline.Fleets.CompleteArrivals(new GameTime(3d), completed);

            Assert.That(timeline.Fleets.HasFleetAt(home), Is.False);
            Assert.That(timeline.Fleets.HasFleetAt(forge), Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(home), Is.False);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(forge), Is.False);
            Assert.That(timeline.Fleets.IsFleetTraveling(first), Is.False);
            Assert.That(timeline.Fleets.TryGetFleet(first, out var moved), Is.True);
            Assert.That(moved.LocationSystemId, Is.EqualTo(forge));
            Assert.That(timeline.Fleets.FleetsAt(forge), Is.EqualTo(new[] { second, first }));
            Assert.That(timeline.Fleets.FleetsAt(home), Is.Empty);
        }

        [Test]
        public void DuplicateClassStacks_CombineAndRemoveOnlyTheRequestedCount()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            var screen = ContentId.From("screen");
            var home = ContentId.From("home-system");
            Assert.That(
                timeline.Fleets.TryAddFleet(
                    new[] { new ShipStack(screen, 1), new ShipStack(screen, 1) },
                    home,
                    out var fleetId),
                Is.True);

            Assert.That(timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.Stacks, Has.Count.EqualTo(1));
            Assert.That(fleet.CountOf(screen), Is.EqualTo(2));
            Assert.That(timeline.Fleets.TryRemoveShips(fleetId, screen, 1), Is.True);
            Assert.That(timeline.Fleets.TryGetFleet(fleetId, out fleet), Is.True);
            Assert.That(fleet.ShipCount, Is.EqualTo(1));

            Assert.That(timeline.Fleets.TryAddShips(fleetId, screen, 2), Is.True);
            Assert.That(timeline.Fleets.TrySplitFleet(fleetId, screen, 1, out var splitId), Is.True);
            Assert.That(timeline.Fleets.TryGetFleet(fleetId, out fleet), Is.True);
            Assert.That(fleet.ShipCount, Is.EqualTo(2));
            Assert.That(timeline.Fleets.TryGetFleet(splitId, out var split), Is.True);
            Assert.That(split.CountOf(screen), Is.EqualTo(1));
        }
    }
}
