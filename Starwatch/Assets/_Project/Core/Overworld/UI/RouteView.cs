using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Single Painter2D layer for all map route segments.
    /// </summary>
    internal sealed class RouteView
    {
        internal VisualElement Root { get; }

        private readonly List<DrawnSegment> _segments = new();
        private float _scale = MapLayout.kDefaultScale;
        private Vector2 _originOffset = MapLayout.kDefaultPadding;

        public RouteView()
        {
            Root = new VisualElement();
            Root.name = "map-route-layer";
            Root.AddToClassList("map-route-layer");
            Root.pickingMode = PickingMode.Ignore;
            Root.style.position = Position.Absolute;
            Root.style.left = 0;
            Root.style.top = 0;
            Root.style.right = 0;
            Root.style.bottom = 0;
            Root.generateVisualContent += OnGenerateVisualContent;
        }

        public int SegmentCount => _segments.Count;

        public void SetTransform(float scale, Vector2 originOffset)
        {
            _scale = scale;
            _originOffset = originOffset;
            Root.MarkDirtyRepaint();
        }

        public void SetSegments(IReadOnlyList<MapRouteSegment> segments)
        {
            _segments.Clear();
            if (segments != null)
            {
                for (var i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    _segments.Add(new DrawnSegment(
                        MapLayout.ToNodeCenter(segment.MapPositionA, _scale, _originOffset),
                        MapLayout.ToNodeCenter(segment.MapPositionB, _scale, _originOffset),
                        segment.State));
                }
            }

            Root.MarkDirtyRepaint();
        }

        public void Clear()
        {
            _segments.Clear();
            Root.MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.lineCap = LineCap.Round;
            for (var i = 0; i < _segments.Count; i++)
            {
                var segment = _segments[i];
                switch (segment.State)
                {
                    case MapRouteState.Planned:
                        painter.strokeColor = MapLayout.kRoutePlannedColor;
                        painter.lineWidth = MapLayout.kRoutePlannedLineWidth;
                        DrawDashed(painter, segment.From, segment.To);
                        break;
                    case MapRouteState.InTransit:
                        painter.strokeColor = MapLayout.kRouteTransitColor;
                        painter.lineWidth = MapLayout.kRouteTransitLineWidth;
                        DrawSolid(painter, segment.From, segment.To);
                        break;
                    default:
                        painter.strokeColor = MapLayout.kRouteKnownColor;
                        painter.lineWidth = MapLayout.kRouteKnownLineWidth;
                        DrawSolid(painter, segment.From, segment.To);
                        break;
                }
            }
        }

        private static void DrawSolid(Painter2D painter, Vector2 from, Vector2 to)
        {
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }

        private static void DrawDashed(Painter2D painter, Vector2 from, Vector2 to)
        {
            var delta = to - from;
            var length = delta.magnitude;
            if (length < 0.01f)
            {
                return;
            }

            var direction = delta / length;
            var drawn = 0f;
            var drawing = true;
            while (drawn < length)
            {
                var step = drawing ? MapLayout.kRouteDashLength : MapLayout.kRouteDashGap;
                var next = Mathf.Min(length, drawn + step);
                if (drawing)
                {
                    painter.BeginPath();
                    painter.MoveTo(from + (direction * drawn));
                    painter.LineTo(from + (direction * next));
                    painter.Stroke();
                }

                drawn = next;
                drawing = !drawing;
            }
        }

        private readonly struct DrawnSegment
        {
            public Vector2 From { get; }

            public Vector2 To { get; }

            public MapRouteState State { get; }

            public DrawnSegment(Vector2 from, Vector2 to, MapRouteState state)
            {
                From = from;
                To = to;
                State = state;
            }
        }
    }
}
