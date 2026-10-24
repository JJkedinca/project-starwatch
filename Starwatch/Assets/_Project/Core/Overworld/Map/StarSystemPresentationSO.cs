using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Authored colors, sizes, and orbit radii for the 3D star-system view.
    /// </summary>
    [CreateAssetMenu(fileName = "StarSystemPresentation", menuName = "Starwatch/Star System Presentation")]
    public sealed class StarSystemPresentationSO : ScriptableObject
    {
        [field: SerializeField]
        public float MinOrbitRadius { get; private set; } = 4f;

        [field: SerializeField]
        public float MaxOrbitRadius { get; private set; } = 12f;

        [field: SerializeField]
        public float CameraBoundsMargin { get; private set; } = 4f;

        [field: SerializeField]
        public float MinPlanetScale { get; private set; } = 0.45f;

        [field: SerializeField]
        public float MaxPlanetScale { get; private set; } = 0.85f;

        [field: SerializeField]
        public float OrbitRingWidth { get; private set; } = 0.08f;

        [field: SerializeField]
        public Color OrbitRing { get; private set; } = new(0.47f, 0.6f, 0.78f, 0.35f);

        [field: SerializeField]
        public Color PlanetUnscanned { get; private set; } = new(0.35f, 0.4f, 0.5f, 1f);

        [field: SerializeField]
        public Color PlanetScanned { get; private set; } = new(0.55f, 0.75f, 0.95f, 1f);

        [field: SerializeField]
        public Color PlanetSelected { get; private set; } = new(0.95f, 0.85f, 0.4f, 1f);

        [field: SerializeField]
        public Color StarColor { get; private set; } = new(1f, 0.92f, 0.65f, 1f);

        internal Color ResolvePlanetColor(PlanetPresentationState state)
        {
            if (state.IsSelected)
            {
                return PlanetSelected;
            }

            return state.Visibility == PlanetVisibility.Scanned ? PlanetScanned : PlanetUnscanned;
        }
    }
}
