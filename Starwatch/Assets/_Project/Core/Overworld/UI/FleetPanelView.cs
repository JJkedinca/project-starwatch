using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Fleet inspection. Hidden until a fleet is selected.
    /// Stack rows stay put while the class list is unchanged, so a split count keeps its value and focus.
    /// </summary>
    internal sealed class FleetPanelView : IFleetPanelView
    {
        public event Action CloseRequested;

        public event Action<ContentId, int> SplitRequested;

        public event Action<EntityId> MergeRequested;

        private readonly List<StackLine> _stackLines = new();
        private readonly List<MergeLine> _mergeLines = new();
        private EntityId _shownFleetId;
        private bool _shownCanSplit;
        private bool _stacksReady;
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

            if (SameStacks(model))
            {
                UpdateStacks(model.Stacks);
                return;
            }

            _stacks.Clear();
            _stackLines.Clear();
            var stacks = model.Stacks;
            var count = stacks?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var row = stacks[i];
                var line = new VisualElement();
                line.AddToClassList("fleet-stack");

                var name = new Label(row.DisplayName);
                name.AddToClassList("fleet-stack-name");
                line.Add(name);

                var detail = new Label(row.Count.ToString());
                detail.AddToClassList("fleet-stack-detail");
                line.Add(detail);

                IntegerField splitCount = null;
                if (model.CanSplit && row.Count > 0)
                {
                    var classId = row.ClassId;
                    splitCount = new IntegerField { value = 1 };
                    splitCount.AddToClassList("fleet-split-count");
                    var countField = splitCount;
                    var split = new Button(() => SplitRequested?.Invoke(classId, countField.value))
                    {
                        text = "Split",
                    };
                    split.AddToClassList("fleet-split");
                    line.Add(splitCount);
                    line.Add(split);
                }

                _stacks.Add(line);
                _stackLines.Add(new StackLine(row.ClassId, name, detail));
            }

            _shownFleetId = model.FleetId;
            _shownCanSplit = model.CanSplit;
            _stacksReady = true;
        }

        private void UpdateStacks(IReadOnlyList<FleetStackRow> stacks)
        {
            for (var i = 0; i < stacks.Count; i++)
            {
                var row = stacks[i];
                var line = _stackLines[i];
                if (line.Name.text != row.DisplayName)
                {
                    line.Name.text = row.DisplayName;
                }

                var detail = row.Count.ToString();
                if (line.Detail.text != detail)
                {
                    line.Detail.text = detail;
                }
            }
        }

        private bool SameStacks(FleetPanelModel model)
        {
            var stacks = model.Stacks;
            var count = stacks?.Count ?? 0;
            if (!_stacksReady
                || model.FleetId != _shownFleetId
                || model.CanSplit != _shownCanSplit
                || _stackLines.Count != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (stacks[i].ClassId != _stackLines[i].ClassId)
                {
                    return false;
                }
            }

            return true;
        }

        private void ShowMerges(IReadOnlyList<FleetMergeCandidate> candidates)
        {
            if (_merges == null)
            {
                return;
            }

            if (SameMerges(candidates))
            {
                UpdateMerges(candidates);
                return;
            }

            _merges.Clear();
            _mergeLines.Clear();
            var count = candidates?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var candidate = candidates[i];
                var fleetId = candidate.FleetId;
                var button = new Button(() => MergeRequested?.Invoke(fleetId))
                {
                    text = "Merge " + candidate.ShipCount,
                };
                button.AddToClassList("fleet-merge");
                _merges.Add(button);
                _mergeLines.Add(new MergeLine(fleetId, button));
            }
        }

        private void UpdateMerges(IReadOnlyList<FleetMergeCandidate> candidates)
        {
            var count = candidates?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var text = "Merge " + candidates[i].ShipCount;
                if (_mergeLines[i].Button.text != text)
                {
                    _mergeLines[i].Button.text = text;
                }
            }
        }

        private bool SameMerges(IReadOnlyList<FleetMergeCandidate> candidates)
        {
            var count = candidates?.Count ?? 0;
            if (_mergeLines.Count != count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (_mergeLines[i].FleetId != candidates[i].FleetId)
                {
                    return false;
                }
            }

            return true;
        }

        private void Hide()
        {
            _stacks?.Clear();
            _merges?.Clear();
            _stackLines.Clear();
            _mergeLines.Clear();
            _stacksReady = false;
            _shownFleetId = EntityId.None;
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        private readonly struct StackLine
        {
            public ContentId ClassId { get; }

            public Label Name { get; }

            public Label Detail { get; }

            public StackLine(ContentId classId, Label name, Label detail)
            {
                ClassId = classId;
                Name = name;
                Detail = detail;
            }
        }

        private readonly struct MergeLine
        {
            public EntityId FleetId { get; }

            public Button Button { get; }

            public MergeLine(EntityId fleetId, Button button)
            {
                FleetId = fleetId;
                Button = button;
            }
        }
    }
}
