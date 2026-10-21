using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Arrow markers on the map surface. Parked markers sit beside nodes;
    /// transit markers animate with a USS translate transition — no layout pass per frame.
    /// </summary>
    internal sealed class FleetMarkerView
    {
        internal VisualElement Root { get; }

        private readonly Dictionary<string, MarkerElement> _markers = new();
        private float _scale = MapLayout.kDefaultScale;
        private Vector2 _originOffset = MapLayout.kDefaultPadding;

        public FleetMarkerView()
        {
            Root = new VisualElement();
            Root.name = "map-fleet-markers";
            Root.AddToClassList("map-fleet-markers");
            Root.pickingMode = PickingMode.Ignore;
            Root.style.position = Position.Absolute;
            Root.style.left = 0;
            Root.style.top = 0;
            Root.style.right = 0;
            Root.style.bottom = 0;
        }

        public int MarkerCount => _markers.Count;

        public void SetTransform(float scale, Vector2 originOffset)
        {
            _scale = scale;
            _originOffset = originOffset;
        }

        public void SetMarkers(IReadOnlyList<FleetMarkerModel> markers)
        {
            HashSet<string> keep = new();
            var count = markers?.Count ?? 0;
            for (var i = 0; i < count; i++)
            {
                var model = markers[i];
                var key = model.Key;
                keep.Add(key);
                if (!_markers.TryGetValue(key, out var element))
                {
                    element = CreateElement(key);
                    _markers[key] = element;
                    Root.Add(element.Root);
                }

                ApplyModel(element, model);
            }

            List<string> remove = null;
            foreach (var pair in _markers)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<string>();
                remove.Add(pair.Key);
            }

            if (remove == null)
            {
                return;
            }

            for (var i = 0; i < remove.Count; i++)
            {
                var key = remove[i];
                if (!_markers.TryGetValue(key, out var element))
                {
                    continue;
                }

                element.Root.RemoveFromHierarchy();
                _markers.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var pair in _markers)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _markers.Clear();
        }

        private void ApplyModel(MarkerElement element, FleetMarkerModel model)
        {
            element.Root.EnableInClassList("fleet-marker--transit", model.IsTransit);
            element.Root.EnableInClassList("fleet-marker--selected", model.IsSelected);
            element.CountLabel.text = model.Count > 1 ? model.Count.ToString() : string.Empty;
            element.CountLabel.style.display = model.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;

            if (model.IsTransit)
            {
                ApplyTransit(element, model);
                return;
            }

            ClearTransition(element.Root);
            SetTranslate(element.Root, ParkedLocal(model.MapPosition));
            element.Root.style.rotate = new Rotate(Angle.Degrees(0f));
        }

        private void ApplyTransit(MarkerElement element, FleetMarkerModel model)
        {
            var from = MapLayout.ToNodeCenter(model.MapPosition, _scale, _originOffset);
            var to = MapLayout.ToNodeCenter(model.DestinationMapPosition, _scale, _originOffset);
            from = MarkerTopLeft(from);
            to = MarkerTopLeft(to);

            var total = model.TotalSeconds;
            var remaining = model.RemainingSeconds;
            var progress = 0f;
            if (total > 0.0001f)
            {
                progress = Mathf.Clamp01(1f - (remaining / total));
            }

            var current = Vector2.Lerp(from, to, progress);
            var angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg + 90f;

            ClearTransition(element.Root);
            SetTranslate(element.Root, current);
            element.Root.style.rotate = new Rotate(Angle.Degrees(angle));

            if (remaining <= 0.0001f)
            {
                SetTranslate(element.Root, to);
                return;
            }

            // Start the USS transition on the next scheduler tick so UI Toolkit
            // sees a from→to translate change after the instantaneous place.
            var duration = remaining;
            var end = to;
            element.Root.schedule.Execute(() =>
            {
                element.Root.style.transitionProperty = new List<StylePropertyName>
                {
                    new StylePropertyName("translate"),
                };
                element.Root.style.transitionDuration = new List<TimeValue>
                {
                    new TimeValue(duration, TimeUnit.Second),
                };
                element.Root.style.transitionTimingFunction = new List<EasingFunction>
                {
                    new EasingFunction(EasingMode.Linear),
                };
                SetTranslate(element.Root, end);
            }).ExecuteLater(0);
        }

        private static void ClearTransition(VisualElement root)
        {
            root.style.transitionDuration = new List<TimeValue>
            {
                new TimeValue(0f, TimeUnit.Second),
            };
        }

        private static void SetTranslate(VisualElement root, Vector2 local)
        {
            root.style.translate = new Translate(local.x, local.y);
        }

        private Vector2 ParkedLocal(Vector2 mapPosition)
        {
            var topLeft = MapLayout.ToLocalPosition(mapPosition, _scale, _originOffset);
            return new Vector2(
                topLeft.x + ((MapLayout.kNodeSize - MapLayout.kFleetMarkerWidth) * 0.5f),
                topLeft.y - MapLayout.kFleetMarkerHeight - MapLayout.kFleetMarkerParkedGap);
        }

        private static Vector2 MarkerTopLeft(Vector2 center)
        {
            return new Vector2(
                center.x - (MapLayout.kFleetMarkerWidth * 0.5f),
                center.y - (MapLayout.kFleetMarkerHeight * 0.5f));
        }

        private static MarkerElement CreateElement(string key)
        {
            var root = new VisualElement { name = "fleet-marker-" + key };
            root.AddToClassList("fleet-marker");
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.top = 0;
            root.style.width = MapLayout.kFleetMarkerWidth;
            root.style.height = MapLayout.kFleetMarkerHeight;

            var arrow = new VisualElement { name = "fleet-marker-arrow" };
            arrow.AddToClassList("fleet-marker__arrow");
            arrow.pickingMode = PickingMode.Ignore;
            root.Add(arrow);

            var count = new Label { name = "fleet-marker-count" };
            count.AddToClassList("fleet-marker__count");
            count.pickingMode = PickingMode.Ignore;
            root.Add(count);

            return new MarkerElement(root, count);
        }

        private sealed class MarkerElement
        {
            internal VisualElement Root { get; }

            internal Label CountLabel { get; }

            internal MarkerElement(VisualElement root, Label countLabel)
            {
                Root = root;
                CountLabel = countLabel;
            }
        }
    }
}
