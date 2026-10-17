using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarSystemModel"/> and toggles the map surface against the star system.
    /// </summary>
    internal sealed class StarSystemScreen : IStarSystemView
    {
        public event Action BackRequested;

        public event Action<ContentId> PlanetSelected;

        public event Action<ContentId> ScanRequested;

        private readonly StarSystemView _system = new();
        private readonly PlanetInspectionView _planet = new();
        private VisualElement _mapSurface;
        private Label _screenTitle;

        internal StarSystemView SystemView => _system;

        internal void Bind(VisualElement starSystem, VisualElement planetDetail, VisualElement mapSurface, Label screenTitle)
        {
            _mapSurface = mapSurface;
            _screenTitle = screenTitle;
            _system.Bind(starSystem);
            _system.BackClicked += ClickBack;
            _system.PlanetClicked += ClickPlanet;
            _planet.Bind(planetDetail);
            _planet.ScanClicked += ClickScan;
        }

        public void Apply(StarSystemModel model)
        {
            if (model == null || !model.IsOpen)
            {
                ShowMap();
                return;
            }

            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.None;
            }

            if (_screenTitle != null)
            {
                _screenTitle.text = "Star System";
            }

            _system.Show(model.Title, model.Planets);
            if (model.ShowPlanetDetail)
            {
                _planet.ShowPlanet(model.PlanetDetail);
            }
            else
            {
                _planet.Clear();
            }
        }

        internal void ClickBack()
        {
            BackRequested?.Invoke();
        }

        internal void ClickPlanet(ContentId planetId)
        {
            PlanetSelected?.Invoke(planetId);
        }

        internal void ClickScan(ContentId planetId)
        {
            ScanRequested?.Invoke(planetId);
        }

        private void ShowMap()
        {
            _system.Hide();
            _planet.Clear();
            if (_mapSurface != null)
            {
                _mapSurface.style.display = DisplayStyle.Flex;
            }

            if (_screenTitle != null)
            {
                _screenTitle.text = "Star Map";
            }
        }
    }
}
