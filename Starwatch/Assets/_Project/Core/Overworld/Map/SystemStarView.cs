using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Central star visual for the open star system. Hosts the system point light.
    /// </summary>
    public sealed class SystemStarView : MonoBehaviour
    {
        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private Renderer _bodyRenderer;

        [SerializeField]
        private Light _pointLight;

        private MaterialPropertyBlock _propertyBlock;

        internal void Apply(Color color)
        {
            if (_bodyRenderer != null)
            {
                _propertyBlock ??= new MaterialPropertyBlock();
                _bodyRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(kBaseColorId, color);
                _bodyRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (_pointLight != null)
            {
                _pointLight.color = color;
            }
        }
    }
}
