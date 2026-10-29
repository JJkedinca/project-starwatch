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
        private static readonly ContentId Home = ContentId.From("home-node");
        private static readonly ContentId Forge = ContentId.From("forge-node");
        private static readonly ContentId Choke = ContentId.From("choke-system");

        [Test]
        public void TryStartTravel_RejectsWrongSourceLocation()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    Home,
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                Forge,
                Choke,
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
                    Home,
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                Home,
                Forge,
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
                    Home,
                    out var fleetId),
                Is.True);

            var travel = new ActiveTravel(
                fleetId,
                Home,
                Forge,
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
                    Home,
                    out var firstFleet),
                Is.True);
            Assert.That(
                timeline.Fleets.TryAddFleet(
                    ContentId.From("screen"),
                    shipCount: 1,
                    Forge,
                    out var secondFleet),
                Is.True);

            var first = new ActiveTravel(
                firstFleet,
                Home,
                Forge,
                GameTime.Zero,
                new GameTime(1d));
            var second = new ActiveTravel(
                secondFleet,
                Forge,
                Choke,
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
            Assert.That(
                timeline.Fleets.TryAddFleet(ContentId.From("screen"), shipCount: 1, Home, out var first),
                Is.True);
            Assert.That(
                timeline.Fleets.TryAddFleet(ContentId.From("screen"), shipCount: 1, Home, out var second),
                Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(Home), Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(Home), Is.False);

            Assert.That(
                timeline.Fleets.TryStartTravel(new ActiveTravel(
                    first,
                    Home,
                    Forge,
                    GameTime.Zero,
                    new GameTime(3d))),
                Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(Home), Is.True);
            Assert.That(timeline.Fleets.IsFleetTraveling(first), Is.True);
            Assert.That(timeline.Fleets.IsFleetTraveling(second), Is.False);

            Assert.That(timeline.Fleets.TrySetFleetLocation(second, Forge), Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(Home), Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(Forge), Is.True);

            System.Collections.Generic.List<ActiveTravel> completed = new();
            timeline.Fleets.CompleteArrivals(new GameTime(3d), completed);

            Assert.That(timeline.Fleets.HasFleetAt(Home), Is.False);
            Assert.That(timeline.Fleets.HasFleetAt(Forge), Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(Home), Is.False);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(Forge), Is.False);
            Assert.That(timeline.Fleets.IsFleetTraveling(first), Is.False);
            Assert.That(timeline.Fleets.TryGetFleet(first, out var moved), Is.True);
            Assert.That(moved.LocationSystemId, Is.EqualTo(Forge));
            Assert.That(timeline.Fleets.FleetsAt(Forge), Is.EqualTo(new[] { second, first }));
            Assert.That(timeline.Fleets.FleetsAt(Home), Is.Empty);
        }

        [Test]
        public void TrySetFleetLocation_RejectsATravelingFleet()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            Assert.That(
                timeline.Fleets.TryAddFleet(ContentId.From("screen"), shipCount: 1, Home, out var fleetId),
                Is.True);
            Assert.That(
                timeline.Fleets.TryStartTravel(new ActiveTravel(
                    fleetId,
                    Home,
                    Forge,
                    GameTime.Zero,
                    new GameTime(3d))),
                Is.True);

            Assert.That(timeline.Fleets.TrySetFleetLocation(fleetId, Forge), Is.False);
            Assert.That(timeline.Fleets.TryGetFleet(fleetId, out var fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(Home));
            Assert.That(timeline.Fleets.IsFleetTraveling(fleetId), Is.True);
            Assert.That(timeline.Fleets.HasTravelingFleetAt(Home), Is.True);
            Assert.That(timeline.Fleets.HasFleetAt(Forge), Is.False);
            Assert.That(timeline.Fleets.ActiveTravels, Has.Count.EqualTo(1));

            System.Collections.Generic.List<ActiveTravel> completed = new();
            Assert.That(timeline.Fleets.CompleteArrivals(new GameTime(3d), completed), Is.EqualTo(1));
            Assert.That(timeline.Fleets.TryGetFleet(fleetId, out fleet), Is.True);
            Assert.That(fleet.LocationSystemId, Is.EqualTo(Forge));
            Assert.That(timeline.Fleets.IsFleetTraveling(fleetId), Is.False);
            Assert.That(timeline.Fleets.ActiveTravels, Is.Empty);
        }

        [Test]
        public void DuplicateClassStacks_CombineAndRemoveOnlyTheRequestedCount()
        {
            var timeline = new TimelineState(new EntityIdFactory());
            var screen = ContentId.From("screen");
            Assert.That(
                timeline.Fleets.TryAddFleet(
                    new[] { new ShipStack(screen, 1), new ShipStack(screen, 1) },
                    Home,
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
