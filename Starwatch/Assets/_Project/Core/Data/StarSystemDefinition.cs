using System;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
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

        [Tooltip("Name shown in fleet lists. Empty falls back to the id.")]
        [SerializeField]
        private string _displayName;

        [field: SerializeField]
        public MapNodeKind Kind { get; private set; }

        [field: SerializeField]
        public Vector2 MapPosition { get; private set; }

        public ContentId Id => ContentId.From(_id);

        public string DisplayName => _displayName ?? string.Empty;

        public StarSystemDefinition()
        {
        }

        public StarSystemDefinition(ContentId id, MapNodeKind kind, Vector2 mapPosition, string displayName = null)
        {
            _id = id.IsEmpty ? string.Empty : id.Value;
            _displayName = displayName ?? string.Empty;
            Kind = kind;
            MapPosition = mapPosition;
        }
    }
}
