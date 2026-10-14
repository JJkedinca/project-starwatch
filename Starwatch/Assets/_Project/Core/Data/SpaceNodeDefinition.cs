using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored map node: a star system or a non-system site.
    /// Planets are listed on PlanetDatabaseSO and matched by system id.
    /// </summary>
    [Serializable]
    public sealed class SpaceNodeDefinition
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private MapNodeKind _kind;

        [SerializeField]
        private Vector2 _mapPosition;

        public ContentId Id => ContentId.From(_id);

        public MapNodeKind Kind => _kind;

        public Vector2 MapPosition => _mapPosition;

        public SpaceNodeDefinition()
        {
        }

        public SpaceNodeDefinition(ContentId id, MapNodeKind kind, Vector2 mapPosition)
        {
            _id = id.IsEmpty ? string.Empty : id.Value;
            _kind = kind;
            _mapPosition = mapPosition;
        }
    }
}
