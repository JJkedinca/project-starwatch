using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Fleet inspection. Hidden until a fleet is selected.
    /// </summary>
    internal sealed class FleetPanelView : IFleetPanelView
    {
        public event Action CloseRequested;

        public event Action<ContentId, int> SplitRequested;

        public event Action<EntityId> MergeRequested;

        private VisualElement _root;
        private Label _title;
        private Label _status;
        private Label _speed;
        private VisualElement _stacks;
        private VisualElement _merges;
        private Button _close;

        /// <summary>
        /// Binds the FleetPanel.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _title = root.Q<Label>("fleet-panel-title");
            _status = root.Q<Label>("fleet-panel-status");
            _speed = root.Q<Label>("fleet-panel-speed");
            _stacks = root.Q<VisualElement>("fleet-panel-stacks");
            _merges = root.Q<VisualElement>("fleet-panel-merges");
            _close = root.Q<Button>("fleet-panel-close");
            if (_close != null)
            {
                _close.clicked += () => CloseRequested?.Invoke();
            }

            Hide();
        }

        public void Apply(FleetPanelModel model)
        {
            if (_root == null || model == null || !model.IsOpen)
            {
                Hide();
                return;
            }

            if (_title != null)
            {
                _title.text = model.LocationId.IsEmpty ? "Fleet" : model.LocationId.Value;
            }

            if (_status != null)
            {
                _status.text = model.IsTraveling ? "Traveling" : "Idle";
            }

            if (_speed != null)
            {
                _speed.text = "Travel speed " + model.TravelSpeed.ToString("0.##");
            }

            ShowStacks(model);
            ShowMerges(model.MergeCandidates);
            _root.style.display = DisplayStyle.Flex;
        }

        private void ShowStacks(FleetPanelModel model)
        {
            if (_stacks == null)
            {
                return;
            }

            _stacks.Clear();
            var stacks = model.Stacks;
            if (stacks == null)
            {
                return;
            }

            for (var i = 0; i < stacks.Count; i++)
            {
                var row = stacks[i];
                var line = new VisualElement();
                line.AddToClassList("fleet-stack");

                var name = new Label(row.DisplayName);
                name.AddToClassList("fleet-stack-name");
                line.Add(name);

                var detail = new Label(Detail(row));
                detail.AddToClassList("fleet-stack-detail");
                line.Add(detail);

                if (model.CanSplit && row.Count > 0)
                {
                    var classId = row.ClassId;
                    var count = new IntegerField { value = 1 };
                    count.AddToClassList("fleet-split-count");
                    var split = new Button(() => SplitRequested?.Invoke(classId, count.value))
                    {
                        text = "Split",
                    };
                    split.AddToClassList("fleet-split");
                    line.Add(count);
                    line.Add(split);
                }

                _stacks.Add(line);
            }
        }

        private void ShowMerges(IReadOnlyList<FleetMergeCandidate> candidates)
        {
            if (_merges == null)
            {
                return;
            }

            _merges.Clear();
            if (candidates == null)
            {
                return;
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var fleetId = candidate.FleetId;
                var button = new Button(() => MergeRequested?.Invoke(fleetId))
                {
                    text = "Merge " + candidate.ShipCount,
                };
                button.AddToClassList("fleet-merge");
                _merges.Add(button);
            }
        }

        private static string Detail(FleetStackRow row)
        {
            return row.Count.ToString();
        }

        private void Hide()
        {
            _stacks?.Clear();
            _merges?.Clear();
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
    }
}
