using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// UITK labels and hover tooltip positioned over world map points.
    /// </summary>
    internal sealed class MapLabelOverlay
    {
        private VisualElement _host;
        private Label _hoverTooltip;
        private Camera _camera;
        private readonly Dictionary<ContentId, Label> _labels = new();
        private readonly Dictionary<ContentId, Vector3> _worldById = new();
        private ContentId _hoveredNodeId;

        internal void Bind(VisualElement host, Camera camera)
        {
            _host = host;
            _camera = camera;
            if (_host == null)
            {
                return;
            }

            _host.pickingMode = PickingMode.Ignore;
            EnsureHoverTooltip();
        }

        internal void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        internal void Rebuild(IReadOnlyList<StarMapNodeModel> nodes, StarMapWorldPresenter renderer)
        {
            if (_host == null)
            {
                return;
            }

            HashSet<ContentId> keep = new();
            var count = nodes?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var model = nodes[i];
                if (model.NodeId.IsEmpty)
                {
                    continue;
                }

                var showLabel = model.State.Visibility != MapNodeVisibility.Hidden
                    && !string.IsNullOrEmpty(model.State.RevealedLabel);
                if (!showLabel)
                {
                    continue;
                }

                keep.Add(model.NodeId);
                if (!_labels.TryGetValue(model.NodeId, out var label))
                {
                    label = CreateLabel(model.NodeId);
                    _labels[model.NodeId] = label;
                    _host.Add(label);
                }

                label.text = model.State.RevealedLabel;
                if (renderer != null && renderer.TryGetNodeWorld(model.NodeId, out var world))
                {
                    _worldById[model.NodeId] = world;
                }
                else
                {
                    _worldById[model.NodeId] = MapSpace.ToWorld(model.MapPosition);
                }
            }

            List<ContentId> remove = null;
            foreach (var pair in _labels)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<ContentId>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    var id = remove[i];
                    _labels[id].RemoveFromHierarchy();
                    _labels.Remove(id);
                    _worldById.Remove(id);
                }
            }

            RepositionAll();
        }

        internal void SetNodeState(ContentId nodeId, MapNodePresentationState state, StarMapWorldPresenter renderer)
        {
            var showLabel = state.Visibility != MapNodeVisibility.Hidden
                && !string.IsNullOrEmpty(state.RevealedLabel);
            if (!showLabel)
            {
                if (_labels.TryGetValue(nodeId, out var existing))
                {
                    existing.RemoveFromHierarchy();
                    _labels.Remove(nodeId);
                    _worldById.Remove(nodeId);
                }

                return;
            }

            if (!_labels.TryGetValue(nodeId, out var label))
            {
                label = CreateLabel(nodeId);
                _labels[nodeId] = label;
                _host?.Add(label);
            }

            label.text = state.RevealedLabel;
            if (renderer != null && renderer.TryGetNodeWorld(nodeId, out var world))
            {
                _worldById[nodeId] = world;
            }

            RepositionLabel(nodeId, label);
        }

        internal void SetHover(ContentId nodeId)
        {
            _hoveredNodeId = nodeId;
        }

        internal void SetHoverTooltip(StarMapHoverTooltipModel tooltip)
        {
            EnsureHoverTooltip();
            if (_hoverTooltip == null)
            {
                return;
            }

            if (!tooltip.IsShown || _hoveredNodeId.IsEmpty || !_worldById.ContainsKey(_hoveredNodeId))
            {
                // Hovered unknown/explorable nodes still need a world point from renderer pick set.
                if (!tooltip.IsShown || _hoveredNodeId.IsEmpty)
                {
                    _hoverTooltip.style.display = DisplayStyle.None;
                    return;
                }
            }

            if (!tooltip.IsShown)
            {
                _hoverTooltip.style.display = DisplayStyle.None;
                return;
            }

            _hoverTooltip.text = tooltip.Reachable
                ? tooltip.TargetName + " · " + tooltip.Seconds.ToString("0.#") + "s"
                : tooltip.TargetName + " · unreachable";
            RepositionHover();
            _hoverTooltip.style.display = DisplayStyle.Flex;
            _hoverTooltip.BringToFront();
        }

        internal void Clear()
        {
            foreach (var pair in _labels)
            {
                pair.Value.RemoveFromHierarchy();
            }

            _labels.Clear();
            _worldById.Clear();
            _hoveredNodeId = default;
            if (_hoverTooltip != null)
            {
                _hoverTooltip.style.display = DisplayStyle.None;
            }
        }

        internal void RepositionAll()
        {
            foreach (var pair in _labels)
            {
                RepositionLabel(pair.Key, pair.Value);
            }

            RepositionHover();
        }

        internal void RememberWorld(ContentId nodeId, Vector3 world)
        {
            _worldById[nodeId] = world;
        }

        private void RepositionLabel(ContentId nodeId, Label label)
        {
            if (_camera == null || _host == null || !_worldById.TryGetValue(nodeId, out var world))
            {
                return;
            }

            if (!TryWorldToHost(world, out var local))
            {
                return;
            }

            label.style.left = local.x + 10f;
            label.style.top = local.y - 10f;
        }

        private void RepositionHover()
        {
            if (_hoverTooltip == null
                || _camera == null
                || _host == null
                || _hoveredNodeId.IsEmpty
                || !_worldById.TryGetValue(_hoveredNodeId, out var world))
            {
                return;
            }

            if (!TryWorldToHost(world, out var local))
            {
                return;
            }

            _hoverTooltip.style.left = local.x + 18f;
            _hoverTooltip.style.top = local.y - 8f;
        }

        private bool TryWorldToHost(Vector3 world, out Vector2 local)
        {
            local = default;
            if (_host == null || _camera == null || _host.panel == null)
            {
                return false;
            }

            var panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(_host.panel, world, _camera);
            local = _host.WorldToLocal(panelPos);
            return true;
        }

        private void EnsureHoverTooltip()
        {
            if (_host == null || _hoverTooltip != null)
            {
                return;
            }

            _hoverTooltip = new Label { name = "map-hover-tooltip" };
            _hoverTooltip.AddToClassList("map-hover-tooltip");
            _hoverTooltip.pickingMode = PickingMode.Ignore;
            _hoverTooltip.style.position = Position.Absolute;
            _hoverTooltip.style.display = DisplayStyle.None;
            _host.Add(_hoverTooltip);
        }

        private static Label CreateLabel(ContentId nodeId)
        {
            var label = new Label { name = "map-label-" + nodeId.Value };
            label.AddToClassList("map-world-label");
            label.pickingMode = PickingMode.Ignore;
            label.style.position = Position.Absolute;
            return label;
        }
    }
}
