using System;
using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Player-facing galaxy size entry: id, display label, and placement/route settings.
    /// </summary>
    [Serializable]
    public sealed class GalaxySizeOption
    {
        [Tooltip("Size id used in run start configuration.")]
        [SerializeField]
        private GalaxySizeId _id = GalaxySizeId.Medium;

        [Tooltip("Label shown in the new-run size dropdown. Empty uses the enum name.")]
        [SerializeField]
        private string _displayName;

        [Tooltip("Placement and route parameters for this size.")]
        [SerializeField]
        private GenerationSettingsSO _settings;

        public GalaxySizeId Id => _id;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName) ? _id.ToString() : _displayName;

        public GenerationSettingsSO Settings => _settings;

        public bool IsValid => _settings != null;

        public GalaxySizeOption()
        {
        }

        public GalaxySizeOption(GalaxySizeId id, string displayName, GenerationSettingsSO settings)
        {
            _id = id;
            _displayName = displayName ?? string.Empty;
            _settings = settings;
        }
    }
}
