using System;
using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Effects;
using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// Pure output and upkeep for one colony. Views and the tick both use this quote.
    /// </summary>
    internal sealed class ProductionService
    {
        private readonly SettlementCatalogSO _catalog;
        private readonly IEffectQuery _effects;

        public ProductionService(ObjectDatabaseSO objectDatabase, IEffectQuery effects = null)
        {
            if (objectDatabase == null)
            {
                throw new ArgumentNullException(nameof(objectDatabase));
            }

            _catalog = objectDatabase.SettlementCatalog
                ?? throw new ArgumentNullException(nameof(objectDatabase.SettlementCatalog));
            _effects = effects;
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

                AddRates(output, definition.Output, placed.Level, planet, PlanetScale.Apply);
                AddRates(upkeep, definition.Upkeep, placed.Level, planet, PlanetScale.Ignore);
            }

            return new ProductionQuote(output, upkeep);
        }

        /// <summary>
        /// Level-1 output of every catalog building after this planet's modifiers, deposits,
        /// and the empire's completed-research output multipliers.
        /// </summary>
        public ResourceInventory ModifiedBase(PlanetDefinition planet)
        {
            var totals = new ResourceInventory();
            var buildings = _catalog.Buildings;
            for (var i = 0; i < buildings.Count; i++)
            {
                var definition = buildings[i];
                if (definition == null)
                {
                    continue;
                }

                AddRates(totals, definition.Output, 1, planet, PlanetScale.Apply);
            }

            return totals;
        }

        private void AddRates(
            ResourceInventory inventory,
            IReadOnlyList<ResourceAmount> rates,
            int level,
            PlanetDefinition planet,
            PlanetScale scale)
        {
            if (rates == null || level <= 0)
            {
                return;
            }

            for (var i = 0; i < rates.Count; i++)
            {
                var rate = rates[i];
                if (rate.Amount <= 0f)
                {
                    continue;
                }

                var multiplier = scale == PlanetScale.Apply ? Multiplier(planet, rate.Kind) : 1f;
                inventory.Add(rate.Kind, rate.Amount * level * multiplier);
            }
        }

        private float Multiplier(PlanetDefinition planet, ResourceKind kind)
        {
            var multiplier = planet != null ? planet.Modifiers.Multiplier(kind) : 1f;
            if (planet != null && planet.HasDeposit(kind))
            {
                multiplier += SettlementCosts.DepositMultiplierBonus;
            }

            var research = _effects != null ? _effects.OutputMultiplier(kind) : 1f;
            if (research <= 0f)
            {
                research = 1f;
            }

            return multiplier * research;
        }

        private enum PlanetScale
        {
            Ignore,
            Apply,
        }
    }
}
