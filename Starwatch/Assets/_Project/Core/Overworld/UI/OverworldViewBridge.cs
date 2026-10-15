using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// UI Toolkit host for the overworld screen. Owns the star map, the star-system
    /// planet strip, star-system inspection, and planet inspection, and raises the
    /// select, travel, scan, and back intents those views produce.
    /// </summary>
    public sealed class OverworldViewBridge : MonoBehaviour
    {
        internal event Action<ContentId> NodeSelected;

        internal event Action TravelRequested;

        internal event Action<ContentId> ScanPlanetRequested;

        internal event Action StarSystemBackRequested;

        internal event Action<ContentId> PlanetSelected;

        internal StarMapView StarMap { get; } = new();

        internal StarSystemView StarSystemView { get; } = new();

        internal StarSystemInspectionView StarSystemInspection { get; } = new();

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
            var starSystem = root.Q<VisualElement>("star-system");
            var starSystemInspection = root.Q<VisualElement>("star-system-inspection");
            var planetInspection = root.Q<VisualElement>("planet-detail");
            if (_mapSurface == null
                || starSystem == null
                || starSystemInspection == null
                || planetInspection == null)
            {
                Debug.LogError(
                    "OverworldViewBridge could not find map-surface, star-system, star-system-inspection, and planet-detail.");
                return;
            }

            StarMap.Bind(_mapSurface);
            StarMap.NodeClicked += id => NodeSelected?.Invoke(id);

            StarSystemView.Bind(starSystem);
            StarSystemView.BackClicked += () => StarSystemBackRequested?.Invoke();
            StarSystemView.PlanetClicked += id => PlanetSelected?.Invoke(id);

            StarSystemInspection.Bind(starSystemInspection);
            StarSystemInspection.TravelClicked += () => TravelRequested?.Invoke();

            PlanetInspection.Bind(planetInspection);
            PlanetInspection.ScanClicked += id => ScanPlanetRequested?.Invoke(id);

            _isBound = true;
            ShowStarMap();
        }

        internal void NotifyNodeSelected(ContentId nodeId)
        {
            NodeSelected?.Invoke(nodeId);
        }

        internal void NotifyTravelRequested()
        {
            TravelRequested?.Invoke();
        }

        internal void NotifyPlanetSelected(ContentId planetId)
        {
            PlanetSelected?.Invoke(planetId);
        }

        internal void NotifyScanPlanetRequested(ContentId planetId)
        {
            ScanPlanetRequested?.Invoke(planetId);
        }

        internal void NotifyStarSystemBackRequested()
        {
            StarSystemBackRequested?.Invoke();
        }

        /// <summary>
        /// Shows the star map and hides the star system.
        /// </summary>
        internal void ShowStarMap()
        {
            StarSystemView.Hide();
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
        /// Shows the star system over the map panel.
        /// </summary>
        internal void ShowStarSystemView(string title, IReadOnlyList<ContentId> planetIds)
        {
            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.None;
            }

            StarSystemView.Show(title, planetIds);
            if (_titleLabel != null)
            {
                _titleLabel.text = "Star System";
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
