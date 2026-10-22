using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Applies node presentation state with MaterialPropertyBlock tints on shared materials.
    /// </summary>
    public sealed class MapNodeView : MonoBehaviour
    {
        private static readonly int kBaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private Renderer _glowRenderer;

        [SerializeField]
        private GameObject _selectionRing;

        [SerializeField]
        private GameObject _fleetIndicator;

        private MaterialPropertyBlock _propertyBlock;

        internal void ApplyState(MapNodePresentationState state, Color color)
        {
            var hidden = state.Visibility == MapNodeVisibility.Hidden && !state.IsExplorable;
            gameObject.SetActive(!hidden);
            if (hidden)
            {
                return;
            }

            if (_glowRenderer != null)
            {
                _propertyBlock ??= new MaterialPropertyBlock();
                _glowRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(kBaseColorId, color);
                _glowRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (_selectionRing != null)
            {
                _selectionRing.SetActive(state.IsSelected);
            }

            if (_fleetIndicator != null)
            {
                _fleetIndicator.SetActive(state.HasFleet);
            }
        }
    }
}
