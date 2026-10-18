using System;
using System.Collections.Generic;
using EndlessHeros.Core.Data;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Fractional totals for a production quote. A missing kind is zero.
    /// Whole-unit methods convert with floor, so a total of 2.9 counts as 2 resources.
    /// </summary>
    public sealed class ResourceInventory
    {
        private readonly Dictionary<ResourceKind, float> _amounts = new();

        /// <summary>
        /// Fractional total for this kind.
        /// </summary>
        public float Get(ResourceKind kind)
        {
            return _amounts.TryGetValue(kind, out var amount) ? amount : 0f;
        }

        /// <summary>
        /// Whole resources in this total. The fraction below 1 is excluded.
        /// </summary>
        public int GetWhole(ResourceKind kind)
        {
            return ToWhole(Get(kind));
        }

        public void Clear()
        {
            _amounts.Clear();
        }

        /// <summary>
        /// Drops one kind so it is not deposited with the other totals.
        /// </summary>
        public void Remove(ResourceKind kind)
        {
            _amounts.Remove(kind);
        }

        /// <summary>
        /// Adds a fractional amount. Non-positive amounts are ignored.
        /// </summary>
        public void Add(ResourceKind kind, float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _amounts.TryGetValue(kind, out var current);
            _amounts[kind] = current + amount;
        }

        public void Add(ResourceInventory other)
        {
            if (other == null)
            {
                return;
            }

            foreach (var pair in other._amounts)
            {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Removes whole resources from the fractional total.
        /// </summary>
        public void Subtract(ResourceKind kind, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var next = Get(kind) - amount;
            if (next <= 0f)
            {
                _amounts.Remove(kind);
            }
            else
            {
                _amounts[kind] = next;
            }
        }

        public bool HasAny()
        {
            foreach (var pair in _amounts)
            {
                if (pair.Value > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when an entry is an unknown kind or a negative total.
        /// </summary>
        internal bool HasMalformedEntry()
        {
            foreach (var pair in _amounts)
            {
                if (pair.Value < 0f || !ResourceKinds.IsDefined(pair.Key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when every whole amount in <paramref name="cost"/> is covered. A fraction below 1 does not add to the cost.
        /// </summary>
        public bool Covers(ResourceInventory cost)
        {
            if (cost == null)
            {
                return true;
            }

            foreach (var pair in cost._amounts)
            {
                if (ToWhole(pair.Value) > GetWhole(pair.Key))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes each whole amount in <paramref name="cost"/> from this inventory.
        /// </summary>
        public void Subtract(ResourceInventory cost)
        {
            if (cost == null)
            {
                return;
            }

            foreach (var pair in cost._amounts)
            {
                Subtract(pair.Key, ToWhole(pair.Value));
            }
        }

        /// <summary>
        /// Grants each whole total into the stockpile. This inventory is left unchanged.
        /// </summary>
        public void DepositFlooredInto(ResourceLedger ledger)
        {
            if (ledger == null)
            {
                return;
            }

            foreach (var pair in _amounts)
            {
                ledger.Add(pair.Key, ToWhole(pair.Value));
            }
        }

        /// <summary>
        /// Drops the whole part of each total and keeps the fraction below 1.
        /// </summary>
        public void RetainFraction()
        {
            for (var i = 0; i < ResourceKinds.All.Count; i++)
            {
                var kind = ResourceKinds.All[i];
                var whole = GetWhole(kind);
                if (whole > 0)
                {
                    Subtract(kind, whole);
                }
            }
        }

        private static int ToWhole(float amount)
        {
            if (amount <= 0f)
            {
                return 0;
            }

            return (int)MathF.Floor(amount);
        }
    }
}
