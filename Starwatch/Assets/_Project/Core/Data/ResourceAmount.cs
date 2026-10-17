using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored or computed quantity of one resource.
    /// </summary>
    [Serializable]
    public struct ResourceAmount
    {
        [Tooltip("Which resource this entry counts.")]
        [field: SerializeField]
        public ResourceKind Kind { get; private set; }

        [Tooltip("How many of that resource.")]
        [field: SerializeField]
        public int Amount { get; private set; }

        public ResourceAmount(ResourceKind kind, int amount)
        {
            Kind = kind;
            Amount = amount;
        }

        /// <summary>
        /// Sums every entry of <paramref name="kind"/> in the list.
        /// </summary>
        public static int Total(IReadOnlyList<ResourceAmount> amounts, ResourceKind kind)
        {
            var total = 0;
            if (amounts == null)
            {
                return 0;
            }

            for (var i = 0; i < amounts.Count; i++)
            {
                if (amounts[i].Kind == kind)
                {
                    total += amounts[i].Amount;
                }
            }

            return total;
        }
    }
}
