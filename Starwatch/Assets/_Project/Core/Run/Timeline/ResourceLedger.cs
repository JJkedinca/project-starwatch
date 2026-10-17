using System;
using System.Collections.Generic;
using EndlessHeros.Core.Data;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Timeline-owned stockpile of whole resources. Spend is all-or-nothing across the quoted cost.
    /// </summary>
    public sealed class ResourceLedger
    {
        private readonly ResourceInventory _amounts = new();

        public int Get(ResourceKind kind)
        {
            return _amounts.GetWhole(kind);
        }

        /// <summary>
        /// Adds a faction's opening stockpile.
        /// </summary>
        public void Grant(StartingResources resources)
        {
            Add(ResourceKind.Industry, resources.Industry);
            Add(ResourceKind.Energy, resources.Energy);
            Add(ResourceKind.Science, resources.Science);
            Add(ResourceKind.Strategic, resources.Strategic);
        }

        /// <summary>
        /// Adds stock from a bundle. Entries of one kind are summed, then the whole part is granted.
        /// A total of 0.6 + 0.6 grants 1. The fraction below 1 is not banked.
        /// </summary>
        public void Grant(IReadOnlyList<ResourceAmount> amounts)
        {
            if (amounts == null)
            {
                return;
            }

            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                Add(kinds[i], WholePart(ResourceAmount.Total(amounts, kinds[i])));
            }
        }

        public void Add(ResourceKind kind, int amount)
        {
            _amounts.Add(kind, amount);
        }

        public bool CanAfford(IReadOnlyList<ResourceAmount> costs)
        {
            if (!IsPayable(costs))
            {
                return false;
            }

            for (var i = 0; i < ResourceKinds.All.Count; i++)
            {
                if (!CanAfford(costs, ResourceKinds.All[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Subtracts every cost entry when the ledger can pay all of them.
        /// </summary>
        public bool TrySpend(IReadOnlyList<ResourceAmount> costs)
        {
            if (!CanAfford(costs))
            {
                return false;
            }

            for (var i = 0; i < ResourceKinds.All.Count; i++)
            {
                Spend(costs, ResourceKinds.All[i]);
            }

            return true;
        }

        /// <summary>
        /// Pays aggregated upkeep from a tick.
        /// </summary>
        public bool TrySpend(ResourceInventory costs)
        {
            if (costs == null)
            {
                return true;
            }

            if (costs.HasMalformedEntry())
            {
                return false;
            }

            if (!costs.HasAny())
            {
                return true;
            }

            if (!_amounts.Covers(costs))
            {
                return false;
            }

            _amounts.Subtract(costs);
            return true;
        }

        private static bool IsPayable(IReadOnlyList<ResourceAmount> costs)
        {
            if (costs == null)
            {
                return true;
            }

            for (var i = 0; i < costs.Count; i++)
            {
                if (costs[i].Amount < 0f || !ResourceKinds.IsDefined(costs[i].Kind))
                {
                    return false;
                }
            }

            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                if (!IsWhole(ResourceAmount.Total(costs, kinds[i])))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanAfford(IReadOnlyList<ResourceAmount> costs, ResourceKind kind)
        {
            return WholeCost(ResourceAmount.Total(costs, kind)) <= Get(kind);
        }

        private void Spend(IReadOnlyList<ResourceAmount> costs, ResourceKind kind)
        {
            _amounts.Subtract(kind, WholeCost(ResourceAmount.Total(costs, kind)));
        }

        private static int WholePart(float amount)
        {
            if (amount <= 0f)
            {
                return 0;
            }

            return (int)MathF.Floor(amount);
        }

        private static int WholeCost(float amount)
        {
            if (!IsWhole(amount) || amount <= 0f)
            {
                return 0;
            }

            return (int)MathF.Round(amount);
        }

        private static bool IsWhole(float amount)
        {
            var nearest = MathF.Round(amount);
            return MathF.Abs(amount - nearest) < 0.001f;
        }
    }
}
