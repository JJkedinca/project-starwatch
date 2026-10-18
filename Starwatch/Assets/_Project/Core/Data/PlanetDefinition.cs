using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored planet facts: inspection copy, output modifiers, footprint, and deposits.
    /// </summary>
    [Serializable]
    public sealed class PlanetDefinition
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private string _systemId;

        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _summary;

        [field: SerializeField]
        public PlanetModifiers Modifiers { get; private set; }

        [SerializeField]
        private int _footprintBudget;

        [SerializeField]
        private List<ResourceKind> _deposits = new();

        public ContentId Id => ContentId.From(_id);

        public ContentId SystemId => ContentId.From(_systemId);

        public string DisplayName => _displayName ?? string.Empty;

        public string Summary => _summary ?? string.Empty;

        public int FootprintBudget => _footprintBudget < 0 ? 0 : _footprintBudget;

        public PlanetDefinition()
        {
        }

        public PlanetDefinition(
            ContentId id,
            ContentId systemId,
            string displayName,
            string summary,
            int footprintBudget = 0,
            IReadOnlyList<ResourceKind> deposits = null,
            PlanetModifiers modifiers = default)
        {
            _id = id.IsEmpty ? string.Empty : id.Value;
            _systemId = systemId.IsEmpty ? string.Empty : systemId.Value;
            _displayName = displayName ?? string.Empty;
            _summary = summary ?? string.Empty;
            Modifiers = modifiers;
            _footprintBudget = footprintBudget < 0 ? 0 : footprintBudget;
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

        public bool HasDeposit(ResourceKind kind)
        {
            for (var i = 0; i < _deposits.Count; i++)
            {
                if (_deposits[i] == kind)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
