using System;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// Pure output and upkeep for one colony. Views and the tick both use this quote.
    /// </summary>
    internal sealed class ProductionService
    {
        private readonly SettlementCatalogSO _catalog;

        public ProductionService(ObjectDatabaseSO objectDatabase)
        {
            if (objectDatabase == null)
            {
                throw new ArgumentNullException(nameof(objectDatabase));
            }

            _catalog = objectDatabase.SettlementCatalog
                ?? throw new ArgumentNullException(nameof(objectDatabase.SettlementCatalog));
        }

        /// <summary>
        /// Returns one second of output and upkeep. Outposts produce nothing.
        /// </summary>
        public ProductionQuote Quote(ColonyState colony, PlanetDefinition planet)
        {
            var output = new ResourceInventory();
            var upkeep = new ResourceInventory();
            if (colony == null || colony.Presence != ColonyPresence.Colony)
            {
                return new ProductionQuote(output, upkeep);
            }

            foreach (var placed in colony.Buildings.Values)
            {
                if (!_catalog.TryGetBuilding(placed.BuildingId, out var definition))
                {
                    continue;
                }

                var multiplier = Multiplier(planet, definition.OutputKind);
                var produced = definition.OutputPerLevel * placed.Level * multiplier;
                output.Add(definition.OutputKind, produced);
                upkeep.Add(definition.UpkeepKind, definition.UpkeepPerLevel * placed.Level);
            }

            return new ProductionQuote(output, upkeep);
        }

        private static float Multiplier(PlanetDefinition planet, ResourceKind kind)
        {
            var multiplier = planet != null ? planet.Modifiers.Multiplier(kind) : 1f;
            if (planet != null && planet.HasDeposit(kind))
            {
                multiplier += SettlementCosts.DepositMultiplierBonus;
            }

            return multiplier;
        }
    }
}
