using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Minimal authored planet facts for inspection/scanning in Phase 1.
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

        public ContentId Id => ContentId.From(_id);

        public ContentId SystemId => ContentId.From(_systemId);

        public string DisplayName => _displayName ?? string.Empty;

        public string Summary => _summary ?? string.Empty;

        public PlanetDefinition()
        {
        }

        public PlanetDefinition(ContentId id, ContentId systemId, string displayName, string summary)
        {
            _id = id.IsEmpty ? string.Empty : id.Value;
            _systemId = systemId.IsEmpty ? string.Empty : systemId.Value;
            _displayName = displayName ?? string.Empty;
            _summary = summary ?? string.Empty;
        }
    }
}
