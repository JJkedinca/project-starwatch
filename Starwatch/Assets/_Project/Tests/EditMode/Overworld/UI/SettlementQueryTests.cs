using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Settlement;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
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
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(35));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database));

            var model = query.Describe(run, Viewing("home-system", ContentId.From("home-prime")));

            Assert.That(model.ShowPanel, Is.True);
            Assert.That(model.Resources.GetWhole(ResourceKind.Industry), Is.EqualTo(80));
            Assert.That(model.Resources.GetWhole(ResourceKind.Energy), Is.EqualTo(40));
            Assert.That(model.Resources.GetWhole(ResourceKind.Science), Is.EqualTo(0));
            Assert.That(model.Commitment.CanFoundOutpost, Is.True);
            Assert.That(model.Commitment.CanFoundColony, Is.False);
            Assert.That(model.Buildings.Count, Is.EqualTo(4));
            Assert.That(BuildingRow(model, "industry-works").CanPlace, Is.False);

            var homePrime = ContentId.From("home-prime");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, homePrime);
            var revealed = query.Describe(run, Viewing("home-system", homePrime));
            Assert.That(revealed.Development, Is.Not.EqualTo(model.Development));
        }

        [Test]
        public void Describe_HidesThePanelOnTheStarMap()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(36));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database));

            var model = query.Describe(run, new OverworldSelection());

            Assert.That(model.ShowPanel, Is.False);
        }

        [Test]
        public void Describe_QuotesPlacedProductionAndTheNextColonyPrice()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(37));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database));
            var homePrime = ContentId.From("home-prime");
            stack.Sensor.ScanPlanet(run, ObserverId.Player, homePrime);
            stack.Settlement.FoundColony(run, ObserverId.Player, homePrime);
            stack.Buildings.Place(run, homePrime, ContentId.From("industry-works"));

            var forge = ContentId.From("forge-world");
            stack.Exploration.Explore(run, ObserverId.Player, ContentId.From("forge-system"));
            stack.Sensor.ScanPlanet(run, ObserverId.Player, forge);

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
        public void Describe_DisablesActionsTheCommandWouldReject()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(38));
            var database = RunTestFixture.CreateObjectDatabase();
            var query = new SettlementQuery(database, new ProductionService(database));
            var homePrime = ContentId.From("home-prime");
            run.Timeline.Resources.TrySpend(new[] { new ResourceAmount(ResourceKind.Industry, 71) });

            var broke = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(broke.Commitment.CanFoundOutpost, Is.False);
            Assert.That(broke.Commitment.CanFoundColony, Is.False);

            run.Timeline.Resources.Grant(new StartingResources(industry: 200));
            stack.Sensor.ScanPlanet(run, ObserverId.Player, homePrime);
            stack.Settlement.FoundColony(run, ObserverId.Player, homePrime);
            stack.Buildings.Place(run, homePrime, ContentId.From("industry-works"));
            stack.Buildings.Place(run, homePrime, ContentId.From("power-plant"));
            run.Timeline.Resources.TrySpend(new[]
            {
                new ResourceAmount(ResourceKind.Industry, run.Timeline.Resources.Get(ResourceKind.Industry)),
            });

            var full = query.Describe(run, Viewing("home-system", homePrime));

            Assert.That(BuildingRow(full, "laboratory").CanPlace, Is.False);
            Assert.That(BuildingRow(full, "industry-works").CanUpgrade, Is.False);

            run.Timeline.Resources.Grant(new StartingResources(industry: 100));
            stack.Buildings.Upgrade(run, homePrime, ContentId.From("industry-works"));
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
