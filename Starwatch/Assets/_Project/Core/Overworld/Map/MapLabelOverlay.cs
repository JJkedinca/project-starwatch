using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Galaxy-map adapter over <see cref="WorldLabelOverlay"/> plus hover tooltip.
    /// </summary>
    internal sealed class MapLabelOverlay
    {
        private readonly WorldLabelOverlay _labels = new();
        private VisualElement _host;
        private Label _hoverTooltip;
        private Camera _camera;
        private ContentId _hoveredNodeId;

        internal void Bind(VisualElement host, Camera camera)
        {
            _host = host;
            _camera = camera;
            _labels.Bind(host, camera);
            EnsureHoverTooltip();
        }

        internal void SetCamera(Camera camera)
        {
            _camera = camera;
            _labels.SetCamera(camera);
        }

        internal void Rebuild(IReadOnlyList<StarMapNodeModel> nodes, StarMapWorldView renderer)
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
                Vector3 world;
                if (renderer != null && renderer.TryGetNodeWorld(model.NodeId, out world))
                {
                    // use renderer world
                }
                else
                {
                    world = MapSpace.ToWorld(model.MapPosition);
                }

                _labels.SetLabel(model.NodeId, model.State.RevealedLabel, world);
            }

            _labels.RetainOnly(keep);
            _labels.RepositionAll();
            RepositionHover();
        }

        internal void SetNodeState(ContentId nodeId, MapNodePresentationState state, StarMapWorldView renderer)
        {
            var showLabel = state.Visibility != MapNodeVisibility.Hidden
                && !string.IsNullOrEmpty(state.RevealedLabel);
            if (!showLabel)
            {
                _labels.RemoveLabel(nodeId);
                return;
            }

            Vector3 world = default;
            var hasWorld = renderer != null && renderer.TryGetNodeWorld(nodeId, out world);
            if (!hasWorld && !_labels.TryGetWorld(nodeId, out world))
            {
                world = default;
            }

            _labels.SetLabel(nodeId, state.RevealedLabel, world);
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

            if (!tooltip.IsShown || _hoveredNodeId.IsEmpty)
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
            _labels.Clear();
            _hoveredNodeId = default;
            if (_hoverTooltip != null)
            {
                _hoverTooltip.style.display = DisplayStyle.None;
            }
        }

        internal void RepositionAll()
        {
            _labels.RepositionAll();
            RepositionHover();
        }

        internal void RememberWorld(ContentId nodeId, Vector3 world)
        {
            _labels.RememberWorld(nodeId, world);
        }

        private void RepositionHover()
        {
            if (_hoverTooltip == null
                || _camera == null
                || _host == null
                || _hoveredNodeId.IsEmpty
                || !_labels.TryGetWorld(_hoveredNodeId, out var world)
                || _host.panel == null)
            {
                return;
            }

            var panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(_host.panel, world, _camera);
            var local = _host.WorldToLocal(panelPos);
            _hoverTooltip.style.left = local.x + 18f;
            _hoverTooltip.style.top = local.y - 8f;
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
    }
}
