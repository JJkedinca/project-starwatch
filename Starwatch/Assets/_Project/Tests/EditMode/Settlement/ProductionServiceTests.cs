using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Settlement;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

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
    }
}
