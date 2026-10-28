using System;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored star-system archetype for generation. Runtime instances get ids and positions at spawn time.
    /// </summary>
    [Serializable]
    public sealed class StarSystemTemplate
    {
        [Tooltip("Name shown on the map and in fleet lists. Empty falls back to the generated id.")]
        [SerializeField]
        private string _displayName;

        [field: SerializeField]
        public MapNodeKind Kind { get; private set; } = MapNodeKind.System;

        public string DisplayName => _displayName ?? string.Empty;

        public StarSystemTemplate()
        {
        }

        public StarSystemTemplate(string displayName, MapNodeKind kind = MapNodeKind.System)
        {
            _displayName = displayName ?? string.Empty;
            Kind = kind;
        }
    }
}
