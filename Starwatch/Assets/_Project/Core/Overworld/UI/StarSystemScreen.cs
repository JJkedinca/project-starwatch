using System;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Applies a <see cref="StarSystemModel"/> and toggles the map surface against the star system.
    /// </summary>
    internal sealed class StarSystemScreen : IStarSystemView
    {
        public event Action BackRequested;

        public event Action<ContentId> PlanetSelected;

        public event Action<EntityId> FleetSelected;

        private readonly StarSystemView _system = new();
        private VisualElement _mapSurface;
        private Label _screenTitle;

        internal StarSystemView SystemView => _system;

        internal void Bind(VisualElement starSystem, VisualElement mapSurface, Label screenTitle)
        {
            _mapSurface = mapSurface;
            _screenTitle = screenTitle;
            _system.Bind(starSystem);
            _system.BackClicked += ClickBack;
            _system.PlanetClicked += ClickPlanet;
            _system.FleetClicked += ClickFleet;
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
            _system.SetFleets(model.Fleets);
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

        private void ShowMap()
        {
            _system.Hide();
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
