using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored star system on the map.
    /// Planets are listed on PlanetDatabaseSO and matched by star system id.
    /// </summary>
    [Serializable]
    public sealed class StarSystemDefinition
    {
        [SerializeField]
        private string _id;

        [field: SerializeField]
        public MapNodeKind Kind { get; private set; }

        [field: SerializeField]
        public Vector2 MapPosition { get; private set; }

        public ContentId Id => ContentId.From(_id);

        public StarSystemDefinition()
        {
        }

        public StarSystemDefinition(ContentId id, MapNodeKind kind, Vector2 mapPosition)
        {
            _id = id.IsEmpty ? string.Empty : id.Value;
            Kind = kind;
            MapPosition = mapPosition;
        }
    }
}
