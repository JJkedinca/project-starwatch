using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Draws one known route as a line on the map surface.
    /// </summary>
    internal sealed class RouteView
    {
        internal VisualElement Root { get; }

        private Vector2 _fromLocal;
        private Vector2 _toLocal;
        private Vector2 _layerOffset;

        public RouteView()
        {
            Root = new VisualElement();
            Root.name = "map-route";
            Root.AddToClassList("map-route");
            Root.pickingMode = PickingMode.Ignore;
            Root.generateVisualContent += OnGenerateVisualContent;
        }

        /// <summary>
        /// Sets line endpoints in map-surface local pixels (node centers).
        /// </summary>
        public void SetLocalEndpoints(Vector2 fromLocal, Vector2 toLocal)
        {
            _fromLocal = fromLocal;
            _toLocal = toLocal;

            var min = Vector2.Min(fromLocal, toLocal);
            var max = Vector2.Max(fromLocal, toLocal);
            var pad = MapLayout.kRoutePad;
            _layerOffset = new Vector2(min.x - pad, min.y - pad);
            Root.style.left = _layerOffset.x;
            Root.style.top = _layerOffset.y;
            Root.style.width = Mathf.Max(1f, max.x - min.x) + (pad * 2f);
            Root.style.height = Mathf.Max(1f, max.y - min.y) + (pad * 2f);
            Root.MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var from = _fromLocal - _layerOffset;
            var to = _toLocal - _layerOffset;

            var painter = context.painter2D;
            painter.strokeColor = MapLayout.kRouteColor;
            painter.lineWidth = MapLayout.kRouteLineWidth;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }
    }
}
