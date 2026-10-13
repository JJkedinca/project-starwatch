using System;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Minimal authored planet facts for inspection/scanning in Phase 1.
    /// </summary>
    [Serializable]
    public sealed class PlanetDefinition
    {
        public ContentId Id { get; }

        public ContentId SystemId { get; }

        public string DisplayName { get; }

        public string Summary { get; }

        public PlanetDefinition(ContentId id, ContentId systemId, string displayName, string summary)
        {
            Id = id;
            SystemId = systemId;
            DisplayName = displayName ?? string.Empty;
            Summary = summary ?? string.Empty;
        }
    }
}
