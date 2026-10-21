using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Fleet rows for the map list and the open system.
    /// </summary>
    public sealed class FleetListQueryTests
    {
        [Test]
        public void AtLocation_OmitsFleetsElsewhereAndMarksTheSelection()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(63));
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var homeFleet = run.Timeline.Fleets[0].Id;
            Assert.That(run.Timeline.Fleets.TryAddFleet(ContentId.From("line-ship"), 2, forge, out var away), Is.True);
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new FleetListQuery(database, stack.Effects, stack.Clock);

            var atHome = query.AtLocation(run, home, homeFleet);
            var all = query.All(run, away);

            Assert.That(atHome, Has.Count.EqualTo(1));
            Assert.That(atHome[0].FleetId, Is.EqualTo(homeFleet));
            Assert.That(atHome[0].Route.LocationId, Is.EqualTo(home));
            Assert.That(
                atHome[0].Route.LocationName,
                Is.EqualTo(database.TryGetStarSystem(home.Value, out var homeSystem) ? homeSystem.DisplayName : home.Value));
            Assert.That(atHome[0].IsSelected, Is.True);
            Assert.That(atHome[0].Route.IsTraveling, Is.False);
            Assert.That(atHome[0].TravelSpeed, Is.EqualTo(1f).Within(0.001f));
            Assert.That(database.ShipCatalog.TryGet(ContentId.From("starter-screen"), out var screen), Is.True);
            Assert.That(atHome[0].Composition.Title, Does.Contain(screen.DisplayName));
            Assert.That(atHome[0].Label, Does.Contain(screen.DisplayName));
            Assert.That(atHome[0].Label, Does.Contain(homeSystem.DisplayName));
            Assert.That(atHome[0].Label, Does.Not.Contain(screen.Id.Value));
            Assert.That(atHome[0].Label, Does.Not.Contain(home.Value));
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all[1].FleetId, Is.EqualTo(away));
            Assert.That(all[1].TravelSpeed, Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(database.ShipCatalog.TryGet(ContentId.From("line-ship"), out var line), Is.True);
            Assert.That(database.TryGetStarSystem(forge.Value, out var forgeSystem), Is.True);
            Assert.That(all[1].Composition.Title, Does.Contain(line.DisplayName));
            Assert.That(all[1].Label, Does.Contain(line.DisplayName));
            Assert.That(all[1].Label, Does.Contain(forgeSystem.DisplayName));
            Assert.That(all[1].Label, Does.Not.Contain(line.Id.Value));
            Assert.That(all[1].Label, Does.Not.Contain(forge.Value));
            Assert.That(all[1].Composition.ShipCount, Is.EqualTo(2));
            Assert.That(all[1].IsSelected, Is.True);
            Assert.That(all[0].IsSelected, Is.False);
        }

        [Test]
        public void All_IncludesTransitDestinationAndArrival()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(96));
            var fleetId = run.Timeline.Fleets[0].Id;
            var forge = ContentId.From("forge-system");
            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, forge).Success,
                Is.True);
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new FleetListQuery(database, stack.Effects, stack.Clock);

            var rows = query.All(run, fleetId);

            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].Route.IsTraveling, Is.True);
            Assert.That(rows[0].Route.DestinationId, Is.EqualTo(forge));
            Assert.That(database.TryGetStarSystem(forge.Value, out var forgeSystem), Is.True);
            Assert.That(rows[0].Route.DestinationName, Is.EqualTo(forgeSystem.DisplayName));
            Assert.That(rows[0].Route.ArrivalSeconds, Is.GreaterThan(0f));
        }
    }
}
