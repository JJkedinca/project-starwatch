using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Draws one route segment with a LineRenderer and shared-material property blocks.
    /// </summary>
    public sealed class MapRouteView : MonoBehaviour
    {
        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private LineRenderer _line;

        private MaterialPropertyBlock _propertyBlock;

        internal void Apply(MapRouteSegment segment, Color color, float width)
        {
            if (_line == null)
            {
                return;
            }

            var from = MapSpace.ToWorld(segment.MapPositionA);
            var to = MapSpace.ToWorld(segment.MapPositionB);
            from.y = 0.05f;
            to.y = 0.05f;
            _line.positionCount = 2;
            _line.SetPosition(0, from);
            _line.SetPosition(1, to);

            _line.startWidth = width;
            _line.endWidth = width;

            _propertyBlock ??= new MaterialPropertyBlock();
            _line.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(kBaseColorId, color);
            _line.SetPropertyBlock(_propertyBlock);
            _line.startColor = color;
            _line.endColor = color;
        }
    }
}
