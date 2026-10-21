using System;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Selected-fleet summary along the bottom of the center column.
    /// </summary>
    internal sealed class FleetBarView : IFleetBarView
    {
        public event Action DetailsRequested;

        public event Action CloseRequested;

        private VisualElement _root;
        private Label _name;
        private Label _status;
        private Label _ships;
        private Label _speed;
        private VisualElement _stacks;
        private Button _details;
        private Button _close;

        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _name = root.Q<Label>("fleet-bar-name");
            _status = root.Q<Label>("fleet-bar-status");
            _ships = root.Q<Label>("fleet-bar-ships");
            _speed = root.Q<Label>("fleet-bar-speed");
            _stacks = root.Q<VisualElement>("fleet-bar-stacks");
            _details = root.Q<Button>("fleet-bar-details");
            _close = root.Q<Button>("fleet-bar-close");
            if (_details != null)
            {
                _details.clicked += () => DetailsRequested?.Invoke();
            }

            if (_close != null)
            {
                _close.clicked += () => CloseRequested?.Invoke();
            }

            Hide();
        }

        public void Apply(FleetPanelModel model)
        {
            if (_root == null || model == null || !model.HasFleet)
            {
                Hide();
                return;
            }

            if (_name != null)
            {
                _name.text = Name(model);
            }

            if (_status != null)
            {
                _status.text = Status(model);
            }

            if (_ships != null)
            {
                _ships.text = model.ShipCount + " ships";
            }

            if (_speed != null)
            {
                _speed.text = "Speed " + model.TravelSpeed.ToString("0.##");
            }

            ShowStacks(model);
            _root.style.display = DisplayStyle.Flex;
        }

        private void ShowStacks(FleetPanelModel model)
        {
            if (_stacks == null)
            {
                return;
            }

            _stacks.Clear();
            for (var i = 0; i < model.Stacks.Count; i++)
            {
                var row = model.Stacks[i];
                var chip = new Label(row.DisplayName + " ×" + row.Count);
                chip.AddToClassList("fleet-bar-chip");
                _stacks.Add(chip);
            }
        }

        private void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            _stacks?.Clear();
        }

        private static string Name(FleetPanelModel model)
        {
            if (model.Stacks.Count == 0)
            {
                return "Fleet";
            }

            return model.Stacks[0].DisplayName + (model.Stacks.Count > 1 ? " fleet" : string.Empty);
        }

        private static string Status(FleetPanelModel model)
        {
            var route = model.Route;
            if (route.IsTraveling)
            {
                var destination = string.IsNullOrEmpty(route.DestinationName)
                    ? route.DestinationId.Value
                    : route.DestinationName;
                return "In transit → " + destination + " · " + route.ArrivalSeconds.ToString("0.#") + "s";
            }

            var location = string.IsNullOrEmpty(route.LocationName) ? route.LocationId.Value : route.LocationName;
            return "Idle at " + location;
        }
    }
}
