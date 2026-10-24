using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Applies planet presentation with MaterialPropertyBlock tints on a shared material.
    /// </summary>
    public sealed class SystemPlanetView : MonoBehaviour
    {
        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private Renderer _bodyRenderer;

        [SerializeField]
        private GameObject _selectionRing;

        private MaterialPropertyBlock _propertyBlock;

        internal ContentId PlanetId { get; private set; }

        internal void Bind(ContentId planetId)
        {
            PlanetId = planetId;
        }

        internal void ApplyState(PlanetPresentationState state, Color color, float scale)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            if (_bodyRenderer != null)
            {
                _propertyBlock ??= new MaterialPropertyBlock();
                _bodyRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(kBaseColorId, color);
                _bodyRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (_selectionRing != null)
            {
                _selectionRing.SetActive(state.IsSelected);
            }
        }
    }
}
