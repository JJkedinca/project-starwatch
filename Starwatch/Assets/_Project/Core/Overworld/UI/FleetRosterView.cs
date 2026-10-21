using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Left-column fleet list for an active run. Hidden when there is no run.
    /// Rows are reused by fleet id so transit refreshes do not drop in-flight clicks.
    /// </summary>
    internal sealed class FleetRosterView : IFleetRosterView
    {
        public event Action<EntityId> FleetSelected;

        private VisualElement _root;
        private VisualElement _list;
        private readonly Dictionary<EntityId, RowElements> _rowsById = new();

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

            HashSet<EntityId> keep = new();
            for (var i = 0; i < fleets.Count; i++)
            {
                var row = fleets[i];
                keep.Add(row.FleetId);
                if (!_rowsById.TryGetValue(row.FleetId, out var elements))
                {
                    elements = CreateRow(row.FleetId);
                    _rowsById[row.FleetId] = elements;
                }

                UpdateRow(elements, row);
                if (elements.Root.parent == _list)
                {
                    _list.Remove(elements.Root);
                }

                _list.Add(elements.Root);
            }

            List<EntityId> remove = null;
            foreach (var pair in _rowsById)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<EntityId>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    var id = remove[i];
                    if (!_rowsById.TryGetValue(id, out var elements))
                    {
                        continue;
                    }

                    elements.Root.RemoveFromHierarchy();
                    _rowsById.Remove(id);
                }
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        private void Hide()
        {
            foreach (var pair in _rowsById)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _rowsById.Clear();
            _list?.Clear();
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        private RowElements CreateRow(EntityId fleetId)
        {
            var button = new Button(() => FleetSelected?.Invoke(fleetId));
            button.name = "fleet-roster-row-" + fleetId.Value;
            button.AddToClassList("fleet-roster-row");

            var title = new Label();
            title.AddToClassList("fleet-roster-row__title");
            button.Add(title);

            var status = new Label();
            status.AddToClassList("fleet-roster-row__status");
            button.Add(status);

            var ships = new Label();
            ships.AddToClassList("fleet-roster-row__ships");
            button.Add(ships);

            return new RowElements(button, title, status, ships);
        }

        private static void UpdateRow(RowElements elements, FleetRowModel row)
        {
            elements.Root.EnableInClassList("fleet-roster-row--selected", row.IsSelected);
            elements.Title.text = row.Composition.Title;
            elements.Status.text = StatusText(row.Route);
            elements.Ships.text = row.Composition.ShipCount.ToString();
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

        private sealed class RowElements
        {
            internal Button Root { get; }

            internal Label Title { get; }

            internal Label Status { get; }

            internal Label Ships { get; }

            internal RowElements(Button root, Label title, Label status, Label ships)
            {
                Root = root;
                Title = title;
                Status = status;
                Ships = ships;
            }
        }
    }
}
