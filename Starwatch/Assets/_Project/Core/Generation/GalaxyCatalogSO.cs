using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Authored size and shape choices for seed-driven galaxy generation.
    /// </summary>
    [CreateAssetMenu(fileName = "GalaxyCatalog", menuName = "Starwatch/Map/Generation/Galaxy Catalog")]
    public sealed class GalaxyCatalogSO : ScriptableObject
    {
        public const GalaxySizeId DefaultSizeId = GalaxySizeId.Medium;
        public const GalaxyShapeId DefaultShapeId = GalaxyShapeId.Circle;

        [Tooltip("Galaxy sizes the player can pick (small / medium / large).")]
        [SerializeField]
        private List<GalaxySizeOption> _sizes = new();

        [Tooltip("Galaxy shapes the player can pick.")]
        [SerializeField]
        private List<GalaxyShapeOption> _shapes = new();

        public IReadOnlyList<GalaxySizeOption> Sizes => _sizes;

        public IReadOnlyList<GalaxyShapeOption> Shapes => _shapes;

        /// <summary>
        /// Asserts default size and shape options are authored. Called at project bootstrap.
        /// </summary>
        public void ValidateRequired()
        {
            Assert.IsTrue(
                TryGetSize(DefaultSizeId, out _),
                $"GalaxyCatalogSO requires a valid size option for default id '{DefaultSizeId}'.");
            Assert.IsTrue(
                TryGetShape(DefaultShapeId, out _),
                $"GalaxyCatalogSO requires a valid shape option for default id '{DefaultShapeId}'.");
        }

        /// <summary>
        /// Fills size/shape lists for EditMode tests.
        /// </summary>
        internal void SetContent(List<GalaxySizeOption> sizes, List<GalaxyShapeOption> shapes)
        {
            _sizes = sizes ?? new List<GalaxySizeOption>();
            _shapes = shapes ?? new List<GalaxyShapeOption>();
        }

        public bool TryGetSize(GalaxySizeId sizeId, out GalaxySizeOption option)
        {
            option = null;
            if (_sizes == null)
            {
                return false;
            }

            for (var i = 0; i < _sizes.Count; i++)
            {
                var size = _sizes[i];
                if (size != null && size.IsValid && size.Id == sizeId)
                {
                    option = size;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetShape(GalaxyShapeId shapeId, out GalaxyShapeOption option)
        {
            option = null;
            if (_shapes == null)
            {
                return false;
            }

            for (var i = 0; i < _shapes.Count; i++)
            {
                var shape = _shapes[i];
                if (shape != null && shape.IsValid && shape.Id == shapeId)
                {
                    option = shape;
                    return true;
                }
            }

            return false;
        }
    }
}
