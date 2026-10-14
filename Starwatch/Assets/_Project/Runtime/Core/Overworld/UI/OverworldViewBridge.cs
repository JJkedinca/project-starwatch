using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Thin UI Toolkit bridge for the overworld map. Raises intents and hosts views.
    /// </summary>
    public sealed class OverworldViewBridge : MonoBehaviour
    {
        internal event Action<ContentId> NodeSelected;

        internal event Action ExploreRequested;

        internal event Action<ContentId> ScanPlanetRequested;

        internal StarMapView StarMap { get; } = new();

        internal SystemInspectionView SystemInspection { get; } = new();

        internal PlanetInspectionView PlanetInspection { get; } = new();

        [SerializeField]
        private UIDocument _document;

        private Label _statusLabel;

        /// <summary>
        /// Queries the visual tree and hooks map / inspection intents.
        /// </summary>
        internal void Bind()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }

            if (_document == null || _document.rootVisualElement == null)
            {
                Debug.LogError("OverworldViewBridge requires a UIDocument with a visual tree.");
                return;
            }

            var root = _document.rootVisualElement;
            _statusLabel = root.Q<Label>("status-label");

            var mapSurface = root.Q<VisualElement>("map-surface");
            if (mapSurface == null)
            {
                Debug.LogError("OverworldViewBridge could not find map-surface.");
                return;
            }

            StarMap.Bind(mapSurface);
            StarMap.NodeClicked += id => NodeSelected?.Invoke(id);

            var inspection = root.Q<VisualElement>("inspection-panel");
            SystemInspection.Bind(inspection);
            SystemInspection.ExploreClicked += () => ExploreRequested?.Invoke();

            PlanetInspection.Bind(inspection);
            PlanetInspection.ScanClicked += id => ScanPlanetRequested?.Invoke(id);
        }

        /// <summary>
        /// Updates the header status line.
        /// </summary>
        internal void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message ?? string.Empty;
            }
        }
    }
}
