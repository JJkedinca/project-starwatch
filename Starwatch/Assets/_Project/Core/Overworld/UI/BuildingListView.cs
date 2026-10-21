using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Catalog buildings for the selected colony, filled into an empty host.
    /// </summary>
    internal sealed class BuildingListView
    {
        public event Action<ContentId> PlaceClicked;

        public event Action<ContentId> UpgradeClicked;

        private VisualElement _root;

        /// <summary>
        /// Binds the BuildingList.uxml root. Rows are created in <see cref="Show"/>.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            Hide();
        }

        /// <summary>
        /// Hides the building list and drops its rows.
        /// </summary>
        public void Hide()
        {
            if (_root == null)
            {
                return;
            }

            _root.Clear();
            _root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Shows one row per catalog building. An existing row is updated in place so a
        /// click on Place or Upgrade can refresh without removing that button.
        /// </summary>
        public void Show(IReadOnlyList<BuildingRowModel> rows)
        {
            if (_root == null)
            {
                return;
            }

            if (!SameRows(rows))
            {
                _root.Clear();
                if (rows != null)
                {
                    for (var i = 0; i < rows.Count; i++)
                    {
                        _root.Add(CreateRow(rows[i]));
                    }
                }
            }
            else if (rows != null)
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    UpdateRow(_root[i], rows[i]);
                }
            }

            _root.style.display = DisplayStyle.Flex;
        }

        private bool SameRows(IReadOnlyList<BuildingRowModel> rows)
        {
            var count = rows?.Count ?? 0;
            if (_root.childCount != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (_root[i].name != RowName(rows[i].BuildingId))
                {
                    return false;
                }
            }

            return true;
        }

        private static void UpdateRow(VisualElement row, BuildingRowModel model)
        {
            var label = row.Q<Label>("building-label");
            if (label != null)
            {
                label.text = FormatLabel(model);
            }

            var place = row.Q<Button>("place-button");
            if (place != null)
            {
                place.text = FormatPlace(model);
                place.SetEnabled(model.CanPlace);
            }

            var upgrade = row.Q<Button>("upgrade-button");
            if (upgrade != null)
            {
                upgrade.text = model.UpgradeLabel;
                upgrade.SetEnabled(model.CanUpgrade);
            }
        }

        private static string RowName(ContentId buildingId)
        {
            return "building-" + buildingId.Value;
        }

        private VisualElement CreateRow(BuildingRowModel row)
        {
            var buildingId = row.BuildingId;
            var element = new VisualElement { name = RowName(buildingId) };
            element.AddToClassList("building-row");
            var label = new Label { name = "building-label", text = FormatLabel(row) };
            label.AddToClassList("building-label");
            element.Add(label);

            var place = new Button { name = "place-button", text = FormatPlace(row) };
            place.AddToClassList("action-button");
            place.SetEnabled(row.CanPlace);
            place.clicked += () => PlaceClicked?.Invoke(buildingId);
            element.Add(place);

            var upgrade = new Button { name = "upgrade-button", text = row.UpgradeLabel };
            upgrade.AddToClassList("action-button");
            upgrade.SetEnabled(row.CanUpgrade);
            upgrade.clicked += () => UpgradeClicked?.Invoke(buildingId);
            element.Add(upgrade);
            return element;
        }

        private static string FormatLabel(BuildingRowModel model)
        {
            var text = model.Name + " · footprint " + model.Footprint;
            if (model.Level > 0)
            {
                text += " · Lv " + model.Level;
            }

            return text;
        }

        private static string FormatPlace(BuildingRowModel model)
        {
            return string.IsNullOrEmpty(model.CostLabel)
                ? "Place"
                : "Place · " + model.CostLabel;
        }
    }
}
