using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// UITK labels positioned over world points. Shared by galaxy and star-system spaces.
    /// </summary>
    internal sealed class WorldLabelOverlay
    {
        private VisualElement _host;
        private Camera _camera;
        private readonly Dictionary<ContentId, Label> _labels = new();
        private readonly Dictionary<ContentId, Vector3> _worldById = new();
        private readonly string _labelClassName;

        internal WorldLabelOverlay(string labelClassName = "map-world-label")
        {
            _labelClassName = labelClassName ?? "map-world-label";
        }

        internal void Bind(VisualElement host, Camera camera)
        {
            _host = host;
            _camera = camera;
            if (_host != null)
            {
                _host.pickingMode = PickingMode.Ignore;
            }
        }

        internal void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        internal void SetLabel(ContentId id, string text, Vector3 world)
        {
            if (_host == null || id.IsEmpty || string.IsNullOrEmpty(text))
            {
                RemoveLabel(id);
                return;
            }

            if (!_labels.TryGetValue(id, out var label))
            {
                label = CreateLabel(id);
                _labels[id] = label;
                _host.Add(label);
            }

            label.text = text;
            _worldById[id] = world;
            RepositionLabel(id, label);
        }

        internal void RemoveLabel(ContentId id)
        {
            if (id.IsEmpty || !_labels.TryGetValue(id, out var label))
            {
                return;
            }

            label.RemoveFromHierarchy();
            _labels.Remove(id);
            _worldById.Remove(id);
        }

        internal void RememberWorld(ContentId id, Vector3 world)
        {
            if (id.IsEmpty)
            {
                return;
            }

            _worldById[id] = world;
        }

        internal bool TryGetWorld(ContentId id, out Vector3 world)
        {
            return _worldById.TryGetValue(id, out world);
        }

        internal void Clear()
        {
            foreach (var pair in _labels)
            {
                pair.Value.RemoveFromHierarchy();
            }

            _labels.Clear();
            _worldById.Clear();
        }

        internal void RepositionAll()
        {
            foreach (var pair in _labels)
            {
                RepositionLabel(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Keeps labels whose ids are in <paramref name="keep"/>; removes the rest.
        /// </summary>
        internal void RetainOnly(HashSet<ContentId> keep)
        {
            if (keep == null)
            {
                Clear();
                return;
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

            if (remove == null)
            {
                return;
            }

            for (var i = 0; i < remove.Count; i++)
            {
                RemoveLabel(remove[i]);
            }
        }

        private void RepositionLabel(ContentId id, Label label)
        {
            if (_camera == null || _host == null || !_worldById.TryGetValue(id, out var world))
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

        private Label CreateLabel(ContentId id)
        {
            var label = new Label { name = "world-label-" + id.Value };
            label.AddToClassList(_labelClassName);
            label.pickingMode = PickingMode.Ignore;
            label.style.position = Position.Absolute;
            return label;
        }
    }
}
