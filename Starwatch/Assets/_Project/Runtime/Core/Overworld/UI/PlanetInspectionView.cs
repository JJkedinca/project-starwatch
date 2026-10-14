using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Lists planets for the selected system and exposes scan actions.
    /// </summary>
    internal sealed class PlanetInspectionView
    {
        public event Action<ContentId> ScanClicked;

        private VisualElement _planetList;

        /// <summary>
        /// Binds to the planet-list container from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement inspectionPanel)
        {
            _planetList = inspectionPanel != null
                ? inspectionPanel.Q<VisualElement>("planet-list")
                : null;
        }

        /// <summary>
        /// Clears planet rows.
        /// </summary>
        public void Clear()
        {
            _planetList?.Clear();
        }

        /// <summary>
        /// Rebuilds planet rows for the selected host system.
        /// </summary>
        public void ShowPlanets(IReadOnlyList<PlanetRowModel> planets)
        {
            Clear();
            if (_planetList == null || planets == null)
            {
                return;
            }

            for (var i = 0; i < planets.Count; i++)
            {
                var model = planets[i];
                if (model.PlanetId.IsEmpty)
                {
                    continue;
                }

                var row = new VisualElement();
                row.AddToClassList("planet-row");

                var name = new Label(model.DisplayName);
                name.AddToClassList("planet-name");
                row.Add(name);

                var detail = new Label(model.Detail);
                detail.AddToClassList("planet-detail");
                row.Add(detail);

                if (model.CanScan)
                {
                    var planetId = model.PlanetId;
                    var scanButton = new Button(() => ScanClicked?.Invoke(planetId))
                    {
                        text = "Scan",
                    };
                    scanButton.AddToClassList("action-button");
                    row.Add(scanButton);
                }

                _planetList.Add(row);
            }
        }
    }

    /// <summary>
    /// Presentation model for one planet row in the inspection panel.
    /// </summary>
    internal readonly struct PlanetRowModel
    {
        public ContentId PlanetId { get; }

        public string DisplayName { get; }

        public string Detail { get; }

        public bool CanScan { get; }

        public PlanetRowModel(ContentId planetId, string displayName, string detail, bool canScan)
        {
            PlanetId = planetId;
            DisplayName = displayName ?? string.Empty;
            Detail = detail ?? string.Empty;
            CanScan = canScan;
        }
    }
}
