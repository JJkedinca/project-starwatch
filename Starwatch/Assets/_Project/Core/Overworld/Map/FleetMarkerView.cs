using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Fleet arrow / chevron on the map. Tint via MaterialPropertyBlock.
    /// </summary>
    public sealed class FleetMarkerView : MonoBehaviour
    {
        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private Renderer _renderer;

        [SerializeField]
        private Transform _countBadge;

        private MaterialPropertyBlock _propertyBlock;

        internal void Apply(Color color, int count)
        {
            if (_renderer != null)
            {
                _propertyBlock ??= new MaterialPropertyBlock();
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(kBaseColorId, color);
                _renderer.SetPropertyBlock(_propertyBlock);
            }

            if (_countBadge != null)
            {
                _countBadge.gameObject.SetActive(count > 1);
            }
        }

        internal void SetPose(Vector3 worldPosition, float yawDegrees)
        {
            transform.position = worldPosition + Vector3.up * 0.4f;
            transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }
    }
}
