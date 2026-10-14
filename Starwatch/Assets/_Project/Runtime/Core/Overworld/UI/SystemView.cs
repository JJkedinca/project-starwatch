using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// In-system planet strip shown when the player drills into a map node.
    /// </summary>
    internal sealed class SystemView
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
        /// Binds to system-view controls from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement systemViewRoot)
        {
            _root = systemViewRoot;
            if (_root == null)
            {
                return;
            }

            _title = _root.Q<Label>("system-view-title");
            _backButton = _root.Q<Button>("system-back-button");
            _planetStrip = _root.Q<VisualElement>("planet-strip");
            if (_backButton != null)
            {
                _backButton.clicked += () => BackClicked?.Invoke();
            }

            Hide();
        }

        /// <summary>
        /// Whether the system view is currently shown.
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Shows the system view and rebuilds planet icons.
        /// </summary>
        public void Show(string title, IReadOnlyList<ContentId> planetIds)
        {
            if (_root == null)
            {
                return;
            }

            if (_title != null)
            {
                _title.text = title ?? "System";
            }

            RebuildPlanets(planetIds);
            _root.style.display = DisplayStyle.Flex;
            _isVisible = true;
        }

        /// <summary>
        /// Hides the system view and clears planet icons.
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

        private void RebuildPlanets(IReadOnlyList<ContentId> planetIds)
        {
            ClearPlanets();
            if (_planetStrip == null || planetIds == null)
            {
                return;
            }

            for (var i = 0; i < planetIds.Count; i++)
            {
                var planetId = planetIds[i];
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
