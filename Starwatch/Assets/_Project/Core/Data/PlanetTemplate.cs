using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored planet archetype for generation. Runtime instances get ids at spawn time.
    /// </summary>
    [Serializable]
    public sealed class PlanetTemplate
    {
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _summary;

        [field: SerializeField]
        public PlanetModifiers Modifiers { get; private set; }

        [SerializeField]
        private int _footprintBudget = 10;

        [SerializeField]
        private List<ResourceKind> _deposits = new();

        public string DisplayName => _displayName ?? string.Empty;

        public string Summary => _summary ?? string.Empty;

        public int FootprintBudget => _footprintBudget < 0 ? 0 : _footprintBudget;

        public IReadOnlyList<ResourceKind> Deposits => _deposits;

        public PlanetTemplate()
        {
        }

        public PlanetTemplate(
            string displayName,
            string summary,
            int footprintBudget,
            PlanetModifiers modifiers = default,
            IReadOnlyList<ResourceKind> deposits = null)
        {
            _displayName = displayName ?? string.Empty;
            _summary = summary ?? string.Empty;
            _footprintBudget = footprintBudget < 0 ? 0 : footprintBudget;
            Modifiers = modifiers;
            _deposits = new List<ResourceKind>();
            if (deposits == null)
            {
                return;
            }

            for (var i = 0; i < deposits.Count; i++)
            {
                _deposits.Add(deposits[i]);
            }
        }
    }
}
