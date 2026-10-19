using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Clickable fleet rows. The host is an empty element filled at runtime.
    /// </summary>
    internal sealed class FleetListView
    {
        public event Action<EntityId> FleetSelected;

        private VisualElement _host;

        /// <summary>
        /// Binds the empty fleet-list host.
        /// </summary>
        public void Bind(VisualElement host)
        {
            _host = host;
        }

        /// <summary>
        /// Rebuilds one button per fleet. The list is not kept.
        /// </summary>
        public void Apply(IReadOnlyList<FleetChoiceModel> fleets)
        {
            if (_host == null)
            {
                return;
            }

            _host.Clear();
            if (fleets == null)
            {
                return;
            }

            for (var i = 0; i < fleets.Count; i++)
            {
                var row = fleets[i];
                var fleetId = row.FleetId;
                var button = new Button(() => FleetSelected?.Invoke(fleetId))
                {
                    text = row.Label,
                };
                button.AddToClassList("fleet-choice");
                button.EnableInClassList("fleet-choice--selected", row.IsSelected);
                _host.Add(button);
            }
        }
    }
}
