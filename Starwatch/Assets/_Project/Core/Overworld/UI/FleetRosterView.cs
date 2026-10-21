using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Left-column fleet list for an active run. Hidden when there is no run.
    /// </summary>
    internal sealed class FleetRosterView : IFleetRosterView
    {
        public event Action<EntityId> FleetSelected;

        private VisualElement _root;
        private VisualElement _list;

        /// <summary>
        /// Binds the fleet-roster-host (or the FleetRoster.uxml root).
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _list = root.Q<VisualElement>("fleet-roster-list") ?? root;
            Hide();
        }

        /// <summary>
        /// Shows the roster for <paramref name="fleets"/>. Pass null to hide (no active run).
        /// An empty list keeps the panel visible during a run with no fleets.
        /// </summary>
        public void Apply(IReadOnlyList<FleetRowModel> fleets)
        {
            if (_list == null)
            {
                return;
            }

            if (fleets == null)
            {
                Hide();
                return;
            }

            _list.Clear();
            for (var i = 0; i < fleets.Count; i++)
            {
                _list.Add(BuildRow(fleets[i]));
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        private void Hide()
        {
            _list?.Clear();
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        private VisualElement BuildRow(FleetRowModel row)
        {
            var fleetId = row.FleetId;
            var button = new Button(() => FleetSelected?.Invoke(fleetId));
            button.AddToClassList("fleet-roster-row");
            button.EnableInClassList("fleet-roster-row--selected", row.IsSelected);

            var title = new Label(row.Composition.Title);
            title.AddToClassList("fleet-roster-row__title");
            button.Add(title);

            var status = new Label(StatusText(row.Route));
            status.AddToClassList("fleet-roster-row__status");
            button.Add(status);

            var ships = new Label(row.Composition.ShipCount.ToString());
            ships.AddToClassList("fleet-roster-row__ships");
            button.Add(ships);

            return button;
        }

        private static string StatusText(FleetRouteModel route)
        {
            if (route.IsTraveling)
            {
                var destination = string.IsNullOrEmpty(route.DestinationName)
                    ? route.DestinationId.Value
                    : route.DestinationName;
                return "IN TRANSIT · " + destination + " · " + route.ArrivalSeconds.ToString("0.#") + "s";
            }

            var location = string.IsNullOrEmpty(route.LocationName) ? route.LocationId.Value : route.LocationName;
            return "IDLE · " + location;
        }
    }
}
