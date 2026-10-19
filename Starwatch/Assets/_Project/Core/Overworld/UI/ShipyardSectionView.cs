using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Shipyard classes and build queue inside the settlement panel.
    /// </summary>
    internal sealed class ShipyardSectionView : IShipyardView
    {
        public event Action<ContentId> QueueRequested;

        public event Action<int> CancelRequested;

        private VisualElement _root;
        private VisualElement _classes;
        private VisualElement _queue;

        /// <summary>
        /// Binds the ShipyardSection.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _classes = root.Q<VisualElement>("shipyard-classes");
            _queue = root.Q<VisualElement>("shipyard-queue");
            Hide();
        }

        public void Apply(ShipyardModel model)
        {
            if (_root == null || model == null || !model.IsShown)
            {
                Hide();
                return;
            }

            ShowClasses(model.Classes);
            ShowQueue(model.Queue);
            _root.style.display = DisplayStyle.Flex;
        }

        private void ShowClasses(IReadOnlyList<ShipyardClassRow> rows)
        {
            if (_classes == null)
            {
                return;
            }

            _classes.Clear();
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var line = new VisualElement();
                line.AddToClassList("shipyard-row");
                var name = new Label(ClassText(row));
                name.AddToClassList("shipyard-row-name");
                line.Add(name);

                var classId = row.ClassId;
                var build = new Button(() => QueueRequested?.Invoke(classId))
                {
                    text = "Build",
                };
                build.AddToClassList("shipyard-build");
                build.SetEnabled(!row.IsLocked);
                line.Add(build);
                _classes.Add(line);
            }
        }

        private void ShowQueue(IReadOnlyList<ShipyardQueueRow> rows)
        {
            if (_queue == null)
            {
                return;
            }

            _queue.Clear();
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var line = new VisualElement();
                line.AddToClassList("shipyard-row");
                var name = new Label(row.DisplayName);
                name.AddToClassList("shipyard-row-name");
                line.Add(name);

                if (row.IsHead && row.BuildSeconds > 0f)
                {
                    var done = row.BuildSeconds - row.SecondsRemaining;
                    var fraction = done / row.BuildSeconds;
                    if (fraction < 0f)
                    {
                        fraction = 0f;
                    }
                    else if (fraction > 1f)
                    {
                        fraction = 1f;
                    }

                    var track = new VisualElement();
                    track.AddToClassList("shipyard-progress");
                    var fill = new VisualElement();
                    fill.AddToClassList("shipyard-progress-fill");
                    fill.style.width = Length.Percent(fraction * 100f);
                    track.Add(fill);
                    line.Add(track);
                }

                var index = row.Index;
                var cancel = new Button(() => CancelRequested?.Invoke(index))
                {
                    text = "Cancel",
                };
                cancel.AddToClassList("shipyard-cancel");
                line.Add(cancel);
                _queue.Add(line);
            }
        }

        private static string ClassText(ShipyardClassRow row)
        {
            var text = row.DisplayName + " · " + CostText(row.Costs)
                + " · " + row.BuildSeconds.ToString("0.#") + "s";
            if (row.IsLocked)
            {
                text += " · Requires " + row.UnlockName;
            }

            return text;
        }

        private static string CostText(IReadOnlyList<ResourceAmount> costs)
        {
            if (costs == null || costs.Count == 0)
            {
                return "0";
            }

            var text = costs[0].Amount.ToString("0.#") + " " + costs[0].Kind;
            for (var i = 1; i < costs.Count; i++)
            {
                text += ", " + costs[i].Amount.ToString("0.#") + " " + costs[i].Kind;
            }

            return text;
        }

        private void Hide()
        {
            _classes?.Clear();
            _queue?.Clear();
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
    }
}
