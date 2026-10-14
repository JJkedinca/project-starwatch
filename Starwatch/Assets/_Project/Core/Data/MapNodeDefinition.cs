using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored map node used by ScenarioDefinition factories and assets.
    /// </summary>
    [Serializable]
    public sealed class MapNodeDefinition
    {
        public ContentId Id { get; }

        public MapNodeKind Kind { get; }

        public Vector2 MapPosition { get; }

        public IReadOnlyList<ContentId> PlanetIds { get; }

        public MapNodeDefinition(
            ContentId id,
            MapNodeKind kind,
            Vector2 mapPosition,
            IReadOnlyList<ContentId> planetIds = null)
        {
            Id = id;
            Kind = kind;
            MapPosition = mapPosition;
            PlanetIds = planetIds ?? Array.Empty<ContentId>();
        }
    }
}
