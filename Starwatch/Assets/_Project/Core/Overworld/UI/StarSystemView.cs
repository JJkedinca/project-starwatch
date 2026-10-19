using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using UnityEngine;
using UnityEngine.UIElements;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Orbital scene shown when the player opens a star system: star, tilted orbits, planets, and fleet dock.
    /// </summary>
    internal sealed class StarSystemView
    {
        public event Action BackClicked;

        public event Action<ContentId> PlanetClicked;

        public event Action<EntityId> FleetClicked;

        private const int kOrbitSegments = 96;

        private VisualElement _root;
        private Label _title;
        private Label _summary;
        private Button _backButton;
        private VisualElement _planetStrip;
        private VisualElement _orbitLayer;
        private VisualElement _star;
        private VisualElement _fleetDock;
        private readonly FleetListView _fleets = new();
        private readonly Dictionary<ContentId, PlanetIconView> _planetsById = new();
        private readonly List<PlanetIconView> _planetOrder = new();
        private readonly List<float> _orbitRadii = new();
        private readonly List<PlanetIconView> _depthOrder = new();
        private int _selectedOrbit = -1;
        private bool _isVisible;

        /// <summary>
        /// Binds the StarSystemView.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            _root = root;
            if (_root == null)
            {
                return;
            }

            _title = _root.Q<Label>("star-system-title");
            _summary = _root.Q<Label>("star-system-summary");
            _backButton = _root.Q<Button>("star-system-back");
            _planetStrip = _root.Q<VisualElement>("planet-strip");
            _orbitLayer = _root.Q<VisualElement>("system-orbits");
            _star = _root.Q<VisualElement>("system-star");
            _fleetDock = _root.Q<VisualElement>("system-fleet-dock");
            _fleets.Bind(_root.Q<VisualElement>("fleet-list"));
            _fleets.FleetSelected += fleetId => FleetClicked?.Invoke(fleetId);
            if (_backButton != null)
            {
                _backButton.clicked += () => BackClicked?.Invoke();
            }

            _planetStrip?.RegisterCallback<GeometryChangedEvent>(_ => LayoutPlanets());
            if (_orbitLayer != null)
            {
                _orbitLayer.generateVisualContent += DrawOrbits;
            }

            Hide();
        }

        /// <summary>
        /// Whether the star system is currently shown.
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Shows the star system. Planet icons are recreated only when the set of planets
        /// changes, so a click can refresh without removing that icon.
        /// </summary>
        public void Show(string title, IReadOnlyList<StarSystemPlanetModel> planets)
        {
            if (_root == null)
            {
                return;
            }

            if (_title != null)
            {
                _title.text = string.IsNullOrEmpty(title) ? "Star System" : title;
            }

            if (!SamePlanets(planets))
            {
                RebuildPlanets(planets);
            }

            var count = planets?.Count ?? 0;
            var scanned = 0;
            _selectedOrbit = -1;
            for (var i = 0; i < count; i++)
            {
                var state = planets[i].State;
                SetPlanetState(planets[i].PlanetId, state);
                if (state.Visibility == PlanetVisibility.Scanned)
                {
                    scanned++;
                }

                if (state.IsSelected && _planetsById.TryGetValue(planets[i].PlanetId, out var view))
                {
                    _selectedOrbit = _planetOrder.IndexOf(view);
                }
            }

            if (_summary != null)
            {
                _summary.text = count == 0
                    ? "NO PLANETS DETECTED"
                    : count + (count == 1 ? " BODY" : " BODIES") + "  ·  " + scanned + " SCANNED";
            }

            _root.style.display = DisplayStyle.Flex;
            _isVisible = true;
            _orbitLayer?.MarkDirtyRepaint();
        }

        /// <summary>
        /// Shows the fleets at this system. Rebuilt on each refresh. The dock hides when empty.
        /// </summary>
        public void SetFleets(IReadOnlyList<FleetChoiceModel> fleets)
        {
            _fleets.Apply(fleets);
            if (_fleetDock != null)
            {
                var hasFleets = fleets != null && fleets.Count > 0;
                _fleetDock.style.display = hasFleets ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// Hides the star system and clears planet icons.
        /// </summary>
        public void Hide()
        {
            ClearPlanets();
            SetFleets(Array.Empty<FleetChoiceModel>());
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            _isVisible = false;
        }

        /// <summary>
        /// Applies presentation state to one planet icon.
        /// </summary>
        public void SetPlanetState(ContentId planetId, PlanetPresentationState state)
        {
            if (!_planetsById.TryGetValue(planetId, out var view))
            {
                return;
            }

            view.ApplyState(state);
        }

        /// <summary>
        /// Number of planet icons currently shown.
        /// </summary>
        public int PlanetCount => _planetsById.Count;

        /// <summary>
        /// Returns whether a planet icon exists in the scene.
        /// </summary>
        public bool HasPlanet(ContentId planetId)
        {
            return _planetsById.ContainsKey(planetId);
        }

        private bool SamePlanets(IReadOnlyList<StarSystemPlanetModel> planets)
        {
            var count = planets?.Count ?? 0;
            if (_planetsById.Count != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                var planetId = planets[i].PlanetId;
                if (planetId.IsEmpty || !_planetsById.ContainsKey(planetId))
                {
                    return false;
                }
            }

            return true;
        }

        private void RebuildPlanets(IReadOnlyList<StarSystemPlanetModel> planets)
        {
            ClearPlanets();
            if (_planetStrip == null || planets == null)
            {
                return;
            }

            for (var i = 0; i < planets.Count; i++)
            {
                var planetId = planets[i].PlanetId;
                if (planetId.IsEmpty || _planetsById.ContainsKey(planetId))
                {
                    continue;
                }

                var view = new PlanetIconView(planetId);
                view.Clicked += id => PlanetClicked?.Invoke(id);
                _planetsById[planetId] = view;
                _planetOrder.Add(view);
                _planetStrip.Add(view.Root);
            }

            LayoutPlanets();
        }

        /// <summary>
        /// Places planets on their orbits and sorts them so nearer planets draw over the star.
        /// Runs on rebuild and whenever the scene resizes.
        /// </summary>
        private void LayoutPlanets()
        {
            _orbitRadii.Clear();
            if (_planetStrip == null)
            {
                return;
            }

            var rect = _planetStrip.contentRect;
            if (float.IsNaN(rect.width) || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var center = rect.size * 0.5f;
            var maxRadius = SystemOrbitLayout.MaxOrbitRadius(rect.size);
            var count = _planetOrder.Count;
            for (var i = 0; i < count; i++)
            {
                var view = _planetOrder[i];
                var radius = SystemOrbitLayout.OrbitRadius(i, count, maxRadius);
                var angle = SystemOrbitLayout.OrbitAngle(i, view.PlanetId);
                view.PlaceAt(
                    SystemOrbitLayout.PlanetCenter(center, radius, angle),
                    SystemOrbitLayout.AwayFromStar(angle));
                _orbitRadii.Add(radius);
            }

            SortByDepth(center.y);
            _orbitLayer?.MarkDirtyRepaint();
        }

        private void SortByDepth(float starDepth)
        {
            _depthOrder.Clear();
            _depthOrder.AddRange(_planetOrder);
            _depthOrder.Sort((a, b) => a.Depth.CompareTo(b.Depth));

            var starPlaced = _star == null;
            for (var i = 0; i < _depthOrder.Count; i++)
            {
                if (!starPlaced && _depthOrder[i].Depth >= starDepth)
                {
                    _star.BringToFront();
                    starPlaced = true;
                }

                _depthOrder[i].Root.BringToFront();
            }

            if (!starPlaced)
            {
                _star.BringToFront();
            }
        }

        private void DrawOrbits(MeshGenerationContext context)
        {
            if (_orbitRadii.Count == 0)
            {
                return;
            }

            var rect = context.visualElement.contentRect;
            var center = rect.size * 0.5f;
            var painter = context.painter2D;
            painter.lineWidth = SystemOrbitLayout.kOrbitLineWidth;
            for (var i = 0; i < _orbitRadii.Count; i++)
            {
                painter.strokeColor = i == _selectedOrbit
                    ? SystemOrbitLayout.kSelectedOrbitColor
                    : SystemOrbitLayout.kOrbitColor;
                painter.BeginPath();
                for (var s = 0; s <= kOrbitSegments; s++)
                {
                    var angle = s * (Mathf.PI * 2f / kOrbitSegments);
                    var point = SystemOrbitLayout.PlanetCenter(center, _orbitRadii[i], angle);
                    if (s == 0)
                    {
                        painter.MoveTo(point);
                    }
                    else
                    {
                        painter.LineTo(point);
                    }
                }

                painter.ClosePath();
                painter.Stroke();
            }
        }

        private void ClearPlanets()
        {
            foreach (var pair in _planetsById)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _planetsById.Clear();
            _planetOrder.Clear();
            _orbitRadii.Clear();
            _selectedOrbit = -1;
            _orbitLayer?.MarkDirtyRepaint();
        }
    }
}
