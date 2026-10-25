using System;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Player-facing galaxy shape entry: id, display label, and shape asset.
    /// </summary>
    [Serializable]
    public sealed class GalaxyShapeOption
    {
        [Tooltip("Shape id used in run start configuration.")]
        [SerializeField]
        private GalaxyShapeId _id = GalaxyShapeId.Circle;

        [Tooltip("Label shown in the new-run shape dropdown. Empty uses the enum name.")]
        [SerializeField]
        private string _displayName;

        [Tooltip("Geometry shape used when this option is selected.")]
        [SerializeField]
        private GalaxyShapeSO _shape;

        public GalaxyShapeId Id => _id;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName) ? _id.ToString() : _displayName;

        public GalaxyShapeSO Shape => _shape;

        public bool IsValid => _shape != null;

        public GalaxyShapeOption()
        {
        }

        public GalaxyShapeOption(GalaxyShapeId id, string displayName, GalaxyShapeSO shape)
        {
            _id = id;
            _displayName = displayName ?? string.Empty;
            _shape = shape;
        }
    }
}
