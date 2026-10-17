using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Settlement;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Settlement
{
    /// <summary>
    /// EditMode coverage for a colony production quote.
    /// </summary>
    public sealed class ProductionServiceTests
    {
        [Test]
        public void Quote_ReturnsOutputAndUpkeepOwnedByTheQuote()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var production = new ProductionService(database);
            Assert.That(database.TryGetPlanet("home-prime", out var planet), Is.True);
            var colony = new ColonyState(ContentId.From("home-prime"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(ContentId.From("industry-works"), 1));

            var quote = production.Quote(colony, planet);
            var again = production.Quote(colony, planet);

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(2f));
            Assert.That(quote.Upkeep.Get(ResourceKind.Energy), Is.EqualTo(1f));
            Assert.That(again.Output, Is.Not.SameAs(quote.Output));
            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(2f));
        }

        [Test]
        public void Quote_OutpostProducesNothing()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var production = new ProductionService(database);
            Assert.That(database.TryGetPlanet("home-prime", out var planet), Is.True);
            var outpost = new ColonyState(ContentId.From("home-prime"), ColonyPresence.Outpost);
            outpost.AddBuilding(new ColonyBuildingState(ContentId.From("industry-works"), 1));

            var quote = production.Quote(outpost, planet);

            Assert.That(quote.Output.HasAny(), Is.False);
            Assert.That(quote.Upkeep.HasAny(), Is.False);
        }

        [Test]
        public void ModifiedBase_MultipliesLevelOneOutputByPlanetModifiers()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var production = new ProductionService(database);
            Assert.That(database.TryGetPlanet("forge-world", out var planet), Is.True);
            Assert.That(
                database.SettlementCatalog.TryGetBuilding(ContentId.From("industry-works"), out var industry),
                Is.True);
            Assert.That(
                database.SettlementCatalog.TryGetBuilding(ContentId.From("extractor"), out var extractor),
                Is.True);

            var totals = production.ModifiedBase(planet);

            Assert.That(totals.Get(ResourceKind.Industry), Is.EqualTo(LevelOne(planet, industry, ResourceKind.Industry)));
            Assert.That(totals.Get(ResourceKind.Strategic), Is.EqualTo(LevelOne(planet, extractor, ResourceKind.Strategic)));
            Assert.That(planet.HasDeposit(ResourceKind.Strategic), Is.True);
            Assert.That(
                totals.Get(ResourceKind.Strategic),
                Is.GreaterThan(
                    ResourceAmount.Total(extractor.Output, ResourceKind.Strategic)
                    * planet.Modifiers.Multiplier(ResourceKind.Strategic)));
        }

        [Test]
        public void Quote_ScalesEveryOutputAndLeavesUpkeepUnmodified()
        {
            var building = ScriptableObject.CreateInstance<BuildingDefinitionSO>();
            var catalog = ScriptableObject.CreateInstance<SettlementCatalogSO>();
            var database = ScriptableObject.CreateInstance<ObjectDatabaseSO>();
            try
            {
                building.SetContent(
                    "dual-works",
                    new[]
                    {
                        new ResourceAmount(ResourceKind.Industry, 1f),
                        new ResourceAmount(ResourceKind.Industry, 1.5f),
                        new ResourceAmount(ResourceKind.Science, 0.5f),
                    },
                    new[]
                    {
                        new ResourceAmount(ResourceKind.Energy, 1f),
                        new ResourceAmount(ResourceKind.Strategic, 0.25f),
                    });
                catalog.SetContent(new[] { building });
                database.AssignDatabases(null, null, catalog);
                var production = new ProductionService(database);
                var plain = new PlanetDefinition(
                    ContentId.From("plain"),
                    ContentId.From("plain-system"),
                    "Plain",
                    string.Empty,
                    modifiers: new PlanetModifiers(1f, 1f, 1f, 1f));
                var rich = new PlanetDefinition(
                    ContentId.From("rich"),
                    ContentId.From("rich-system"),
                    "Rich",
                    string.Empty,
                    deposits: new[] { ResourceKind.Industry },
                    modifiers: new PlanetModifiers(2f, 1f, 1f, 1f));
                var colony = new ColonyState(ContentId.From("plain"), ColonyPresence.Colony);
                colony.AddBuilding(new ColonyBuildingState(ContentId.From("dual-works"), 2));

                var quote = production.Quote(colony, plain);
                var richBase = production.ModifiedBase(rich);

                Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(5f));
                Assert.That(quote.Output.Get(ResourceKind.Science), Is.EqualTo(1f));
                Assert.That(quote.Upkeep.Get(ResourceKind.Energy), Is.EqualTo(2f));
                Assert.That(quote.Upkeep.Get(ResourceKind.Strategic), Is.EqualTo(0.5f));
                Assert.That(richBase.Get(ResourceKind.Industry), Is.EqualTo(7.5f));
                Assert.That(richBase.Get(ResourceKind.Science), Is.EqualTo(0.5f));
            }
            finally
            {
                Object.DestroyImmediate(building);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(database);
            }
        }

        private static float LevelOne(PlanetDefinition planet, BuildingDefinitionSO building, ResourceKind kind)
        {
            var multiplier = planet.Modifiers.Multiplier(kind);
            if (planet.HasDeposit(kind))
            {
                multiplier += SettlementCosts.DepositMultiplierBonus;
            }

            return ResourceAmount.Total(building.Output, kind) * multiplier;
        }
    }
}
