using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.Map;
using UnityEngine;
using UnityEngine.UIElements;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarSystemModel"/> to the 3D system world view, labels, and UITK chrome.
    /// Space ownership (roots / camera) lives on <see cref="OverworldSpaceSwitcher"/>.
    /// </summary>
    internal sealed class StarSystemView : IStarSystemView
    {
        public event Action BackRequested;

        public event Action<ContentId> PlanetSelected;

        public event Action<EntityId> FleetSelected;

        private readonly WorldLabelOverlay _labels = new();
        private readonly FleetListView _fleets = new();
        private VisualElement _chromeRoot;
        private VisualElement _mapSurface;
        private VisualElement _rightPanel;
        private VisualElement _fleetDock;
        private Label _screenTitle;
        private Label _systemTitle;
        private Label _systemSummary;
        private Button _backButton;
        private StarSystemWorldView _worldView;
        private MapCameraController _cameraController;
        private SystemPointerInput _pointerInput;
        private OverworldSpaceSwitcher _spaceSwitcher;
        private Camera _camera;
        private bool _wired;
        private bool _chromeBound;

        internal void Bind(
            VisualElement starSystemChrome,
            VisualElement mapSurface,
            Label screenTitle,
            VisualElement rightPanel,
            StarSystemWorldView worldView,
            MapCameraController cameraController,
            SystemPointerInput pointerInput,
            OverworldSpaceSwitcher spaceSwitcher,
            Camera mapCamera)
        {
            _chromeRoot = starSystemChrome;
            _mapSurface = mapSurface;
            _screenTitle = screenTitle;
            _rightPanel = rightPanel;
            _worldView = worldView;
            _cameraController = cameraController;
            _pointerInput = pointerInput;
            _spaceSwitcher = spaceSwitcher;
            _camera = mapCamera;

            BindChrome(starSystemChrome);
            _labels.Bind(mapSurface, mapCamera);

            if (_wired)
            {
                return;
            }

            _wired = true;
            if (_pointerInput != null)
            {
                _pointerInput.PlanetClicked += ClickPlanet;
                _pointerInput.PlanetHovered += OnPlanetHovered;
            }

            if (_cameraController != null)
            {
                _cameraController.Moved += OnCameraMoved;
            }

            if (_worldView != null)
            {
                _worldView.PlanetsRebuilt += OnPlanetsRebuilt;
            }
        }

        public void Apply(StarSystemModel model)
        {
            if (model == null || !model.IsOpen)
            {
                ShowGalaxyChrome();
                _worldView?.Clear();
                _labels.Clear();
                _spaceSwitcher?.ShowGalaxy();
                return;
            }

            _spaceSwitcher?.ShowStarSystem();
            ShowSystemChrome(model.Title, model.Status);
            _worldView?.Rebuild(model.Planets, model.OrbitSlotCount);
            ApplyCameraForSystem();
            RebuildLabels(model);
            SetFleets(model.Fleets);
        }

        internal void ClickBack()
        {
            BackRequested?.Invoke();
        }

        internal void ClickPlanet(ContentId planetId)
        {
            PlanetSelected?.Invoke(planetId);
        }

        internal void ClickFleet(EntityId fleetId)
        {
            FleetSelected?.Invoke(fleetId);
        }

        private void BindChrome(VisualElement root)
        {
            if (root == null || _chromeBound)
            {
                return;
            }

            _chromeBound = true;
            _systemTitle = root.Q<Label>("star-system-title");
            _systemSummary = root.Q<Label>("star-system-summary");
            _backButton = root.Q<Button>("star-system-back");
            _fleetDock = root.Q<VisualElement>("system-fleet-dock");
            _fleets.Bind(root.Q<VisualElement>("fleet-list"));
            _fleets.FleetSelected += fleetId => FleetSelected?.Invoke(fleetId);
            if (_backButton != null)
            {
                _backButton.clicked += () => BackRequested?.Invoke();
            }

            // Hide UITK orbit/planet strip; world view owns those.
            var planetStrip = root.Q<VisualElement>("planet-strip");
            if (planetStrip != null)
            {
                planetStrip.style.display = DisplayStyle.None;
            }

            var orbitLayer = root.Q<VisualElement>("system-orbits");
            if (orbitLayer != null)
            {
                orbitLayer.style.display = DisplayStyle.None;
            }

            var systemStar = root.Q<VisualElement>("system-star");
            if (systemStar != null)
            {
                systemStar.style.display = DisplayStyle.None;
            }
        }

        private void ShowGalaxyChrome()
        {
            if (_chromeRoot != null)
            {
                _chromeRoot.style.display = DisplayStyle.None;
            }

            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.Flex;
            }

            if (_screenTitle != null)
            {
                _screenTitle.text = "Star Map";
            }
        }

        private void ShowSystemChrome(string title, string status)
        {
            if (_mapSurface != null)
            {
                // Keep map-surface visible as the transparent label host over the 3D view.
                _mapSurface.style.display = DisplayStyle.Flex;
            }

            if (_chromeRoot != null)
            {
                _chromeRoot.style.display = DisplayStyle.Flex;
            }

            if (_screenTitle != null)
            {
                _screenTitle.text = "Star System";
            }

            if (_systemTitle != null)
            {
                _systemTitle.text = title ?? string.Empty;
            }

            if (_systemSummary != null)
            {
                _systemSummary.text = status ?? string.Empty;
            }
        }

        private void SetFleets(System.Collections.Generic.IReadOnlyList<FleetRowModel> fleets)
        {
            _fleets.Apply(fleets);
            if (_fleetDock != null)
            {
                var hasFleets = fleets != null && fleets.Count > 0;
                _fleetDock.style.display = hasFleets ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RebuildLabels(StarSystemModel model)
        {
            if (_worldView == null)
            {
                return;
            }

            var keep = new System.Collections.Generic.HashSet<ContentId>();
            var planets = model.Planets;
            for (var i = 0; i < planets.Count; i++)
            {
                var planet = planets[i];
                if (planet.PlanetId.IsEmpty || string.IsNullOrEmpty(planet.State.DisplayName))
                {
                    continue;
                }

                if (!_worldView.TryGetPlanetWorld(planet.PlanetId, out var world))
                {
                    continue;
                }

                keep.Add(planet.PlanetId);
                _labels.SetLabel(planet.PlanetId, planet.State.DisplayName, world);
            }

            _labels.RetainOnly(keep);
            _labels.RepositionAll();
        }

        private void ApplyCameraForSystem()
        {
            if (_spaceSwitcher == null || !_spaceSwitcher.IsSystemActive || _worldView == null)
            {
                return;
            }

            var bounds = _worldView.BuildBounds();
            _cameraController?.SetBounds(bounds);
            if (_cameraController == null)
            {
                return;
            }

            var camera = _cameraController.MapCamera;
            var inset = SystemViewportFraming.MeasureRightPanelInsetNormalized(_rightPanel);
            var focus = SystemViewportFraming.CameraFocusForUncoveredCenter(
                camera,
                _worldView.StarWorld,
                _cameraController.FixedHeight,
                inset);
            _cameraController.CenterOn(focus, instant: true);
        }

        private void OnPlanetHovered(ContentId planetId)
        {
            if (!planetId.IsEmpty
                && _worldView != null
                && _worldView.TryGetPlanetWorld(planetId, out var world))
            {
                _labels.RememberWorld(planetId, world);
                _labels.RepositionAll();
            }
        }

        private void OnCameraMoved()
        {
            if (_spaceSwitcher != null && _spaceSwitcher.IsSystemActive)
            {
                _labels.RepositionAll();
            }
        }

        private void OnPlanetsRebuilt()
        {
            if (_spaceSwitcher != null && _spaceSwitcher.IsSystemActive)
            {
                _labels.RepositionAll();
            }
        }
    }
}
