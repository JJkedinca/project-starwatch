using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored or generated map node used by ScenarioDefinition.
    /// </summary>
    [Serializable]
    public sealed class MapNodeDefinition
    {
        public ContentId Id { get; }

        public MapNodeKind Kind { get; }

        public Vector2 MapPosition { get; }

        /// <summary>
        /// Display label from a system template. Empty falls back to the id at UI time.
        /// </summary>
        public string DisplayName { get; }

        public IReadOnlyList<ContentId> PlanetIds { get; }

        public MapNodeDefinition(
            ContentId id,
            MapNodeKind kind,
            Vector2 mapPosition,
            IReadOnlyList<ContentId> planetIds = null,
            string displayName = null)
        {
            Id = id;
            Kind = kind;
            MapPosition = mapPosition;
            DisplayName = displayName ?? string.Empty;
            PlanetIds = planetIds ?? Array.Empty<ContentId>();
        }
    }
}
