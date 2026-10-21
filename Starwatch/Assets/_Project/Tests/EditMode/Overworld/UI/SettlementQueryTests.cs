using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Settlement panel read model: costs, scan gate, and quoted rates.
    /// </summary>
    public sealed class SettlementQueryTests
    {
        [Test]
        public void Describe_UnscannedPlanetOffersAnOutpostAndHidesTheColony()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(35),
                RunTestFixture.UnsettledVerticalSlice());
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);

            var model = query.Describe(run, Viewing("home-system", ContentId.From("home-prime")));

            Assert.That(model.ShowPanel, Is.True);
            Assert.That(model.Resources.GetWhole(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(model.Resources.GetWhole(ResourceKind.Energy), Is.EqualTo(40));
            Assert.That(model.Resources.GetWhole(ResourceKind.Science), Is.EqualTo(0));
            Assert.That(model.Commitment.CanFoundOutpost, Is.True);
            Assert.That(model.Commitment.CanFoundColony, Is.False);
            Assert.That(model.Commitment.ShowCard, Is.True);
            Assert.That(model.Header.Presence, Is.EqualTo(SettlementPresenceKind.None));
            Assert.That(model.Header.IsScanned, Is.False);
            Assert.That(model.ShowShipyardTab, Is.False);
            Assert.That(model.Buildings.Count, Is.EqualTo(4));
            Assert.That(BuildingRow(model, "industry-works").CanPlace, Is.False);
            Assert.That(BuildingRow(model, "industry-works").Footprint, Is.GreaterThan(0));

            var homePrime = ContentId.From("home-prime");
            stack.SensorService.ScanPlanet(run, ObserverId.Player, homePrime);
            var revealed = query.Describe(run, Viewing("home-system", homePrime));
            Assert.That(revealed.Development, Is.Not.EqualTo(model.Development));
        }

        [Test]
        public void Describe_HidesThePanelOnTheStarMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(36));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);

            var model = query.Describe(run, new OverworldSelection());

            Assert.That(model.ShowPanel, Is.False);
        }

        [Test]
        public void Describe_HidesWhileResearchOrFleetDetailsOwnTheColumn()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(38));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(ContentId.From("home-prime"));

            Assert.That(query.Describe(run, selection).ShowPanel, Is.True);

            selection.OpenResearch();
            Assert.That(query.Describe(run, selection).ShowPanel, Is.False);

            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            selection.OpenFleetDetails();
            Assert.That(query.Describe(run, selection).ShowPanel, Is.False);
        }

        [Test]
        public void Describe_StartingColonyCanBuildWhileDevelopmentStaysHidden()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(92));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var homePrime = ContentId.From("home-prime");

            var model = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(homePrime), Is.False);
            Assert.That(model.Economy.IsRevealed, Is.False);
            Assert.That(model.Header.Level, Is.EqualTo(1));
            Assert.That(model.Header.CanScan, Is.True);
            Assert.That(model.ShowShipyardTab, Is.False);
            Assert.That(BuildingRow(model, "industry-works").CanPlace, Is.True);

            Assert.That(
                stack.BuildingService.Place(run, homePrime, ContentId.From("shipyard")).Success,
                Is.True);
            var withYard = query.Describe(run, Viewing("home-system", homePrime));
            Assert.That(withYard.ShowShipyardTab, Is.True);
        }

        [Test]
        public void Describe_ColonyLevelTracksHighestBuildingLevel()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(93));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var homePrime = ContentId.From("home-prime");
            Assert.That(
                stack.BuildingService.Place(run, homePrime, ContentId.From("industry-works")).Success,
                Is.True);
            Assert.That(
                stack.BuildingService.Upgrade(run, homePrime, ContentId.From("industry-works")).Success,
                Is.True);

            var model = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(model.Header.Presence, Is.EqualTo(SettlementPresenceKind.Colony));
            Assert.That(model.Header.Level, Is.EqualTo(2));
            Assert.That(BuildingRow(model, "industry-works").Level, Is.EqualTo(2));
        }

        [Test]
        public void Describe_QuotesPlacedProductionAndTheNextColonyPrice()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(37),
                RunTestFixture.UnsettledVerticalSlice());
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var homePrime = ContentId.From("home-prime");
            stack.SensorService.ScanPlanet(run, ObserverId.Player, homePrime);
            stack.SettlementService.FoundColony(run, ObserverId.Player, homePrime);
            stack.BuildingService.Place(run, homePrime, ContentId.From("industry-works"));

            var forge = ContentId.From("forge-world");
            stack.ExplorationService.Explore(run, ObserverId.Player, ContentId.From("forge-system"));
            stack.SensorService.ScanPlanet(run, ObserverId.Player, forge);

            var home = query.Describe(run, Viewing("home-system", homePrime));
            var next = query.Describe(run, Viewing("forge-system", forge));

            Assert.That(home.Resources.GetWhole(ResourceKind.Industry), Is.EqualTo(20));
            Assert.That(home.ShowPlanet, Is.True);
            var industry = BuildingRow(home, "industry-works");
            Assert.That(industry.CanPlace, Is.False);
            Assert.That(industry.CanUpgrade, Is.True);
            Assert.That(next.ShowPlanet, Is.True);
            Assert.That(next.Commitment.CanFoundColony, Is.False);
            Assert.That(next.Development, Is.Not.EqualTo(home.Development));
        }

        [Test]
        public void Describe_ResourcesFollowTheSelectedPlanet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(60));
            var database = RunTestFixture.CreateObjectDatabase();
            var productionService = new ProductionService(database);
            var query = new SettlementQuery(database, productionService, stack.ShipProductionService);
            var forge = ContentId.From("forge-system");
            var forgeWorld = ContentId.From("forge-world");
            var innerBelt = ContentId.From("forge-inner-belt");
            run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, forge);
            stack.ExplorationService.Explore(run, ObserverId.Player, forge);
            database.TryGetPlanet(forgeWorld.Value, out var worldPlanet);
            database.TryGetPlanet(innerBelt.Value, out var beltPlanet);

            var hidden = query.Describe(run, Viewing("forge-system", forgeWorld));
            stack.SensorService.ScanPlanet(run, ObserverId.Player, forgeWorld);
            stack.SensorService.ScanPlanet(run, ObserverId.Player, innerBelt);
            var world = query.Describe(run, Viewing("forge-system", forgeWorld));
            var belt = query.Describe(run, Viewing("forge-system", innerBelt));

            Assert.That(hidden.ShowPlanet, Is.True);
            Assert.That(hidden.Economy.IsRevealed, Is.False);
            Assert.That(world.Development, Is.Not.EqualTo(hidden.Development));
            Assert.That(world.Economy.IsRevealed, Is.True);
            Assert.That(belt.Economy.IsRevealed, Is.True);
            Assert.That(
                world.Economy.Base.Get(ResourceKind.Industry),
                Is.EqualTo(productionService.ModifiedBase(worldPlanet).Get(ResourceKind.Industry)));
            Assert.That(
                belt.Economy.Base.Get(ResourceKind.Industry),
                Is.EqualTo(productionService.ModifiedBase(beltPlanet).Get(ResourceKind.Industry)));
            Assert.That(
                world.Economy.Base.Get(ResourceKind.Strategic),
                Is.EqualTo(productionService.ModifiedBase(worldPlanet).Get(ResourceKind.Strategic)));
            Assert.That(
                belt.Economy.Base.Get(ResourceKind.Strategic),
                Is.EqualTo(productionService.ModifiedBase(beltPlanet).Get(ResourceKind.Strategic)));
            Assert.That(
                world.Economy.Base.Get(ResourceKind.Industry),
                Is.Not.EqualTo(belt.Economy.Base.Get(ResourceKind.Industry)));
            Assert.That(world.Economy.Rate(ResourceKind.Industry), Is.EqualTo(0f));
            Assert.That(belt.Economy.Rate(ResourceKind.Industry), Is.EqualTo(0f));
        }

        [Test]
        public void Describe_UpgradeRaisesProductionAndLeavesBase()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(62),
                RunTestFixture.UnsettledVerticalSlice());
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var homePrime = ContentId.From("home-prime");
            stack.SensorService.ScanPlanet(run, ObserverId.Player, homePrime);
            stack.SettlementService.FoundColony(run, ObserverId.Player, homePrime);
            stack.BuildingService.Place(run, homePrime, ContentId.From("industry-works"));

            var placed = query.Describe(run, Viewing("home-system", homePrime));
            var industryBase = placed.Economy.Base.Get(ResourceKind.Industry);
            stack.BuildingService.Upgrade(run, homePrime, ContentId.From("industry-works"));
            var upgraded = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(placed.Economy.Rate(ResourceKind.Industry), Is.EqualTo(industryBase));
            Assert.That(placed.Economy.Rate(ResourceKind.Energy), Is.LessThan(0f));
            Assert.That(upgraded.Economy.Base.Get(ResourceKind.Industry), Is.EqualTo(industryBase));
            Assert.That(upgraded.Economy.Rate(ResourceKind.Industry), Is.EqualTo(industryBase * 2f));
            Assert.That(upgraded.Economy.Rate(ResourceKind.Energy), Is.EqualTo(placed.Economy.Rate(ResourceKind.Energy) * 2f));
        }

        [Test]
        public void Describe_DisablesActionsTheCommandWouldReject()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun(
                "authored-vertical-slice",
                RunSeed.FromValue(38),
                RunTestFixture.UnsettledVerticalSlice());
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database), stack.ShipProductionService);
            var homePrime = ContentId.From("home-prime");
            run.Timeline.Resources.TrySpend(new[] { new ResourceAmount(ResourceKind.Industry, 71) });

            var broke = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(broke.Commitment.CanFoundOutpost, Is.False);
            Assert.That(broke.Commitment.CanFoundColony, Is.False);

            run.Timeline.Resources.Grant(new StartingResources(industry: 200));
            stack.SensorService.ScanPlanet(run, ObserverId.Player, homePrime);
            stack.SettlementService.FoundColony(run, ObserverId.Player, homePrime);
            stack.BuildingService.Place(run, homePrime, ContentId.From("industry-works"));
            stack.BuildingService.Place(run, homePrime, ContentId.From("power-plant"));
            run.Timeline.Resources.TrySpend(new[]
            {
                new ResourceAmount(ResourceKind.Industry, run.Timeline.Resources.Get(ResourceKind.Industry)),
            });

            var full = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(BuildingRow(full, "laboratory").CanPlace, Is.False);
            Assert.That(BuildingRow(full, "industry-works").CanUpgrade, Is.False);

            run.Timeline.Resources.Grant(new StartingResources(industry: 100));
            stack.BuildingService.Upgrade(run, homePrime, ContentId.From("industry-works"));
            var capped = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(BuildingRow(capped, "industry-works").CanUpgrade, Is.False);
            Assert.That(BuildingRow(capped, "laboratory").CanPlace, Is.False);
        }

        private static OverworldSelection Viewing(string nodeId, ContentId planetId)
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From(nodeId));
            if (!planetId.IsEmpty)
            {
                selection.SelectPlanet(planetId);
            }

            return selection;
        }

        private static BuildingRowModel BuildingRow(SettlementPanelModel model, string buildingId)
        {
            var id = ContentId.From(buildingId);
            for (var i = 0; i < model.Buildings.Count; i++)
            {
                if (model.Buildings[i].BuildingId == id)
                {
                    return model.Buildings[i];
                }
            }

            Assert.Fail("Missing building row " + buildingId);
            return default;
        }
    }
}
