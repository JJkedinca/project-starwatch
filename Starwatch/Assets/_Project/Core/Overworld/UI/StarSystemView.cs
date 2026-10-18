using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Planet strip shown when the player opens a star system.
    /// </summary>
    internal sealed class StarSystemView
    {
        public event Action BackClicked;

        public event Action<ContentId> PlanetClicked;

        private VisualElement _root;
        private Label _title;
        private Button _backButton;
        private VisualElement _planetStrip;
        private readonly Dictionary<ContentId, PlanetIconView> _planetsById = new();
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
            _backButton = _root.Q<Button>("star-system-back");
            _planetStrip = _root.Q<VisualElement>("planet-strip");
            if (_backButton != null)
            {
                _backButton.clicked += () => BackClicked?.Invoke();
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
                _title.text = title ?? "Star System";
            }

            if (!SamePlanets(planets))
            {
                RebuildPlanets(planets);
            }

            var count = planets?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                SetPlanetState(planets[i].PlanetId, planets[i].State);
            }

            _root.style.display = DisplayStyle.Flex;
            _isVisible = true;
        }

        /// <summary>
        /// Hides the star system and clears planet icons.
        /// </summary>
        public void Hide()
        {
            ClearPlanets();
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
        /// Returns whether a planet icon exists in the strip.
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
                _planetStrip.Add(view.Root);
            }
        }

        private void ClearPlanets()
        {
            foreach (var pair in _planetsById)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _planetsById.Clear();
            _planetStrip?.Clear();
        }
    }
}
