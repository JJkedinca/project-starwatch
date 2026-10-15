using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.Services;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for the overworld map read model.
    /// </summary>
    public sealed class OverworldMapQueryTests
    {
        [Test]
        public void KnownRoutes_IncludesRoutesOutOfExploredHome()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(41));
            var query = CreateQuery(stack.Visibility, stack.Travel);

            var routes = query.KnownRoutes(run);

            Assert.That(routes, Has.Count.EqualTo(2));
        }

        [Test]
        public void NodeState_KeepsHiddenNeighborExplorableWhileFleetTravels()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(42));
            var query = CreateQuery(stack.Visibility, stack.Travel);
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var fleetId = run.Timeline.Fleets[0].Id;

            var started = stack.Travel.StartTravel(run, ObserverId.Player, fleetId, forge);

            Assert.That(started.Success, Is.True);
            var forgeState = query.NodeState(run, RequireNode(run, forge), ContentId.Empty);
            var homeState = query.NodeState(run, RequireNode(run, home), ContentId.Empty);
            var inspection = query.DescribeSelection(run, forge, ContentId.Empty, inStarSystemView: false);

            Assert.That(forgeState.Visibility, Is.EqualTo(MapNodeVisibility.Hidden));
            Assert.That(forgeState.IsExplorable, Is.True);
            Assert.That(forgeState.RevealedLabel, Is.Empty);
            Assert.That(forgeState.HasFleet, Is.False);
            Assert.That(homeState.HasFleet, Is.True);
            Assert.That(homeState.IsFleetTraveling, Is.True);
            Assert.That(inspection.CanTravel, Is.False);
            Assert.That(inspection.Title, Is.EqualTo("Uncharted space"));
            Assert.That(inspection.Summary, Is.EqualTo("Hidden until a fleet arrives by a known route."));
        }

        [Test]
        public void DescribeSelection_OffersTravelIntoHiddenNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(43));
            var query = CreateQuery(stack.Visibility, stack.Travel);
            var forge = ContentId.From("forge-system");

            var inspection = query.DescribeSelection(run, forge, ContentId.Empty, inStarSystemView: false);

            Assert.That(inspection.HasSelection, Is.True);
            Assert.That(inspection.CanTravel, Is.True);
            Assert.That(inspection.Title, Is.EqualTo("Uncharted space"));
            Assert.That(inspection.Summary, Is.EqualTo("Known route. Travel here to enter."));
            Assert.That(inspection.ShowPlanetDetail, Is.False);
            Assert.That(query.TryFindFleetForDestination(run, forge, out var fleetId), Is.True);
            Assert.That(fleetId, Is.EqualTo(run.Timeline.Fleets[0].Id));
        }

        [Test]
        public void VisiblePlanets_IsEmptyForExploredLocationWithoutPlanets()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(44));
            var query = CreateQuery(stack.Visibility, stack.Travel);
            var belt = ContentId.From("asteroid-belt");

            Assert.That(query.CanEnter(run, belt), Is.False);

            run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
            stack.Exploration.Explore(run, ObserverId.Player, belt);

            Assert.That(query.CanEnter(run, belt), Is.True);
            Assert.That(query.VisiblePlanets(run, RequireNode(run, belt)), Is.Empty);
            Assert.That(query.StarSystemStatus(0), Is.EqualTo("No planets in this system."));
        }

        [Test]
        public void PlanetState_UsesAuthoredNameAndScanCopy()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(45));
            var query = CreateQuery(stack.Visibility, stack.Travel);
            var home = RequireNode(run, ContentId.From("home-system"));
            var prime = ContentId.From("home-prime");

            var row = query.PlanetState(run, home, prime, prime);
            var ready = query.DescribeSelection(run, home.Id, prime, inStarSystemView: true);

            Assert.That(row.DisplayName, Is.EqualTo("Home Prime"));
            Assert.That(row.IsSelected, Is.True);
            Assert.That(row.Visibility, Is.EqualTo(PlanetVisibility.SystemExplored));
            Assert.That(ready.ShowPlanetDetail, Is.True);
            Assert.That(ready.PlanetDetail.DisplayName, Is.EqualTo("Home Prime"));
            Assert.That(ready.PlanetDetail.Detail, Is.EqualTo("Ready to scan."));
            Assert.That(ready.PlanetDetail.CanScan, Is.True);
            Assert.That(ready.Title, Is.EqualTo("home-system"));
            Assert.That(ready.Summary, Is.EqualTo("Star system · home-system\nFleet present."));

            stack.Sensor.ScanPlanet(run, ObserverId.Player, prime);
            var scanned = query.DescribeSelection(run, home.Id, prime, inStarSystemView: true);

            Assert.That(scanned.PlanetDetail.Detail, Is.EqualTo("Temperate starter world."));
            Assert.That(scanned.PlanetDetail.CanScan, Is.False);
            Assert.That(
                query.PlanetState(run, home, prime, ContentId.Empty).Visibility,
                Is.EqualTo(PlanetVisibility.Scanned));
        }

        private static OverworldMapQuery CreateQuery(VisibilityService visibility, TravelService travel)
        {
            return new OverworldMapQuery(visibility, travel, RunTestFixture.CreateObjectDatabase());
        }

        private static MapNode RequireNode(RunState run, ContentId nodeId)
        {
            Assert.That(run.Universe.MapGraph.TryGetNode(nodeId, out var node), Is.True);
            return node;
        }
    }
}
