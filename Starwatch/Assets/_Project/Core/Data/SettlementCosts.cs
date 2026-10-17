using System.Collections.Generic;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Shared settlement prices. Every playable faction uses these until a faction can override them.
    /// </summary>
    public static class SettlementCosts
    {
        private const int kOutpostIndustry = 10;

        private const int kColonyBaseIndustry = 40;

        private const int kColonyStepIndustry = 40;

        private const float kDepositMultiplierBonus = 1f;

        private static readonly ResourceAmount[] kOutpostCost =
        {
            new ResourceAmount(ResourceKind.Industry, kOutpostIndustry),
        };

        public static float DepositMultiplierBonus => kDepositMultiplierBonus;

        public static IReadOnlyList<ResourceAmount> OutpostCost => kOutpostCost;

        /// <summary>
        /// Cost of the next colony. <paramref name="existingColonies"/> does not include outposts.
        /// </summary>
        public static List<ResourceAmount> ColonyCost(int existingColonies)
        {
            if (existingColonies < 0)
            {
                existingColonies = 0;
            }

            var industry = kColonyBaseIndustry + kColonyStepIndustry * existingColonies;
            return new List<ResourceAmount> { new ResourceAmount(ResourceKind.Industry, industry) };
        }
    }
}
