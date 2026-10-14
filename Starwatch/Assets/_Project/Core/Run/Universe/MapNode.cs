using System.Collections.Generic;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Run.Universe
{
    /// <summary>
    /// Immutable runtime map node copied from authored scenario content.
    /// </summary>
    public sealed class MapNode
    {
        public ContentId Id { get; }

        public MapNodeKind Kind { get; }

        public Vector2 MapPosition { get; }

        public IReadOnlyList<ContentId> PlanetIds { get; }

        public MapNode(
            ContentId id,
            MapNodeKind kind,
            Vector2 mapPosition,
            IReadOnlyList<ContentId> planetIds)
        {
            Id = id;
            Kind = kind;
            MapPosition = mapPosition;
            PlanetIds = planetIds ?? System.Array.Empty<ContentId>();
        }
    }
}
