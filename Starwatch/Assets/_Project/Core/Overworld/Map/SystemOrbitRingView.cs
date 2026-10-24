using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Circular orbit ring drawn with a LineRenderer on a shared material.
    /// </summary>
    public sealed class SystemOrbitRingView : MonoBehaviour
    {
        private const int kSegmentCount = 64;

        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private LineRenderer _line;

        private MaterialPropertyBlock _propertyBlock;

        internal void Apply(Vector3 starWorld, float radius, Color color, float width)
        {
            if (_line == null)
            {
                return;
            }

            _line.loop = true;
            _line.positionCount = kSegmentCount;
            _line.startWidth = width;
            _line.endWidth = width;
            for (var i = 0; i < kSegmentCount; i++)
            {
                var t = i / (float)kSegmentCount;
                var angle = t * Mathf.PI * 2f;
                _line.SetPosition(
                    i,
                    new Vector3(
                        starWorld.x + (Mathf.Cos(angle) * radius),
                        starWorld.y + 0.02f,
                        starWorld.z + (Mathf.Sin(angle) * radius)));
            }

            _propertyBlock ??= new MaterialPropertyBlock();
            _line.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(kBaseColorId, color);
            _line.SetPropertyBlock(_propertyBlock);
            _line.startColor = color;
            _line.endColor = color;
        }
    }
}
