using System;
using System.Collections.Generic;
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

        internal event Action SystemBackRequested;

        internal event Action<ContentId> PlanetSelected;

        internal StarMapView StarMap { get; } = new();

        internal SystemView SystemView { get; } = new();

        internal SystemInspectionView SystemInspection { get; } = new();

        internal PlanetInspectionView PlanetInspection { get; } = new();

        [SerializeField]
        private UIDocument _document;

        private Label _statusLabel;
        private Label _titleLabel;
        private VisualElement _mapSurface;
        private bool _isBound;

        /// <summary>
        /// Queries the UIDocument visual tree and hooks map / inspection intents.
        /// </summary>
        internal void Bind()
        {
            if (_isBound)
            {
                return;
            }

            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }

            if (_document == null || _document.rootVisualElement == null)
            {
                Debug.LogError("OverworldViewBridge requires a UIDocument with a visual tree.");
                return;
            }

            Bind(_document.rootVisualElement);
        }

        /// <summary>
        /// Hooks map / inspection intents from an existing visual tree.
        /// </summary>
        internal void Bind(VisualElement root)
        {
            if (_isBound)
            {
                return;
            }

            if (root == null)
            {
                Debug.LogError("OverworldViewBridge requires a visual tree.");
                return;
            }

            _statusLabel = root.Q<Label>("status-label");
            _titleLabel = root.Q<Label>("title");

            _mapSurface = root.Q<VisualElement>("map-surface");
            var systemView = root.Q<VisualElement>("system-view");
            if (_mapSurface == null || systemView == null)
            {
                Debug.LogError("OverworldViewBridge could not find map-surface and system-view.");
                return;
            }

            StarMap.Bind(_mapSurface);
            StarMap.NodeClicked += id => NodeSelected?.Invoke(id);

            SystemView.Bind(systemView);
            SystemView.BackClicked += () => SystemBackRequested?.Invoke();
            SystemView.PlanetClicked += id => PlanetSelected?.Invoke(id);

            var inspection = root.Q<VisualElement>("inspection-panel");
            SystemInspection.Bind(inspection);
            SystemInspection.ExploreClicked += () => ExploreRequested?.Invoke();

            PlanetInspection.Bind(inspection);
            PlanetInspection.ScanClicked += id => ScanPlanetRequested?.Invoke(id);

            _isBound = true;
            ShowStarMap();
        }

        internal void NotifyNodeSelected(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void NotifyExploreRequested()
        {
            ExploreRequested?.Invoke();
        }

        internal void NotifyPlanetSelected(ContentId planetId)
        {
            PlanetSelected?.Invoke(planetId);
        }

        internal void NotifyScanPlanetRequested(ContentId planetId)
        {
            ScanPlanetRequested?.Invoke(planetId);
        }

        internal void NotifySystemBackRequested()
        {
            SystemBackRequested?.Invoke();
        }

        /// <summary>
        /// Shows the star map and hides the system view.
        /// </summary>
        internal void ShowStarMap()
        {
            SystemView.Hide();
            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.Flex;
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = "Star Map";
            }
        }

        /// <summary>
        /// Shows the system view over the map panel.
        /// </summary>
        internal void ShowSystemView(string title, IReadOnlyList<ContentId> planetIds)
        {
            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.None;
            }

            SystemView.Show(title, planetIds);
            if (_titleLabel != null)
            {
                _titleLabel.text = "System";
            }
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
