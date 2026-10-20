using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Shipyard classes and build queue inside the settlement panel.
    /// Rows stay put while only build progress changes, so a click is not dropped on refresh.
    /// </summary>
    internal sealed class ShipyardSectionView : IShipyardView
    {
        public event Action<ContentId> QueueRequested;

        public event Action<int> CancelRequested;

        private readonly List<ShipyardClassRow> _shownClasses = new();
        private readonly List<ShipyardQueueRow> _shownQueue = new();
        private readonly List<VisualElement> _queueFills = new();
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
            if (_classes == null || SameClasses(rows))
            {
                return;
            }

            _classes.Clear();
            _shownClasses.Clear();
            var count = rows?.Count ?? 0;
            for (var i = 0; i < count; i++)
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
                _shownClasses.Add(row);
            }
        }

        private void ShowQueue(IReadOnlyList<ShipyardQueueRow> rows)
        {
            if (_queue == null)
            {
                return;
            }

            if (SameQueue(rows))
            {
                UpdateProgress(rows);
                return;
            }

            _queue.Clear();
            _shownQueue.Clear();
            _queueFills.Clear();
            var count = rows?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var row = rows[i];
                var line = new VisualElement();
                line.AddToClassList("shipyard-row");
                var name = new Label(row.DisplayName);
                name.AddToClassList("shipyard-row-name");
                line.Add(name);

                VisualElement fill = null;
                if (row.IsHead && row.BuildSeconds > 0f)
                {
                    var track = new VisualElement();
                    track.AddToClassList("shipyard-progress");
                    fill = new VisualElement { name = "shipyard-progress-fill" };
                    fill.AddToClassList("shipyard-progress-fill");
                    fill.style.width = Length.Percent(ProgressFraction(row) * 100f);
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
                _shownQueue.Add(row);
                _queueFills.Add(fill);
            }
        }

        private void UpdateProgress(IReadOnlyList<ShipyardQueueRow> rows)
        {
            var count = rows?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var fill = _queueFills[i];
                if (fill == null)
                {
                    continue;
                }

                fill.style.width = Length.Percent(ProgressFraction(rows[i]) * 100f);
            }
        }

        private bool SameClasses(IReadOnlyList<ShipyardClassRow> rows)
        {
            var count = rows?.Count ?? 0;
            if (_shownClasses.Count != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (!SameClass(_shownClasses[i], rows[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool SameQueue(IReadOnlyList<ShipyardQueueRow> rows)
        {
            var count = rows?.Count ?? 0;
            if (_shownQueue.Count != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                var shown = _shownQueue[i];
                var row = rows[i];
                if (shown.Index != row.Index
                    || shown.ClassId != row.ClassId
                    || shown.DisplayName != row.DisplayName
                    || shown.BuildSeconds != row.BuildSeconds
                    || shown.IsHead != row.IsHead)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool SameClass(ShipyardClassRow left, ShipyardClassRow right)
        {
            return left.ClassId == right.ClassId
                && left.DisplayName == right.DisplayName
                && left.BuildSeconds == right.BuildSeconds
                && left.IsLocked == right.IsLocked
                && left.UnlockResearchId == right.UnlockResearchId
                && left.UnlockName == right.UnlockName
                && SameCosts(left.Costs, right.Costs);
        }

        private static bool SameCosts(IReadOnlyList<ResourceAmount> left, IReadOnlyList<ResourceAmount> right)
        {
            var leftCount = left?.Count ?? 0;
            var rightCount = right?.Count ?? 0;
            if (leftCount != rightCount)
            {
                return false;
            }

            for (var i = 0; i < leftCount; i++)
            {
                if (left[i].Kind != right[i].Kind || left[i].Amount != right[i].Amount)
                {
                    return false;
                }
            }

            return true;
        }

        private static float ProgressFraction(ShipyardQueueRow row)
        {
            if (row.BuildSeconds <= 0f)
            {
                return 0f;
            }

            var fraction = (row.BuildSeconds - row.SecondsRemaining) / row.BuildSeconds;
            if (fraction < 0f)
            {
                return 0f;
            }

            if (fraction > 1f)
            {
                return 1f;
            }

            return fraction;
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
            _shownClasses.Clear();
            _shownQueue.Clear();
            _queueFills.Clear();
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
    }
}
