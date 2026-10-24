using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Authored colors and route widths for the 3D star map.
    /// </summary>
    [CreateAssetMenu(fileName = "StarMapPresentation", menuName = "Starwatch/Star Map Presentation")]
    public sealed class StarMapPresentationSO : ScriptableObject
    {
        [field: SerializeField]
        public Color NodeHidden { get; private set; } = new(0f, 0f, 0f, 0f);

        [field: SerializeField]
        public Color NodeExplorable { get; private set; } = new(0.35f, 0.55f, 0.7f, 0.55f);

        [field: SerializeField]
        public Color NodeExplored { get; private set; } = new(0.45f, 0.85f, 1f, 1f);

        [field: SerializeField]
        public Color NodePresent { get; private set; } = new(0.55f, 0.95f, 0.75f, 1f);

        [field: SerializeField]
        public Color RouteKnown { get; private set; } = new(0.35f, 0.72f, 0.82f, 0.55f);

        [field: SerializeField]
        public Color RoutePlanned { get; private set; } = new(0.90f, 0.75f, 0.28f, 0.95f);

        [field: SerializeField]
        public Color RouteTransit { get; private set; } = new(0.95f, 0.55f, 0.20f, 0.95f);

        [field: SerializeField]
        public Color FleetMarker { get; private set; } = new(0.95f, 0.85f, 0.35f, 1f);

        [field: SerializeField]
        public Color FleetMarkerSelected { get; private set; } = new(1f, 0.95f, 0.55f, 1f);

        [field: SerializeField]
        public float RouteKnownWidth { get; private set; } = 0.12f;

        [field: SerializeField]
        public float RoutePlannedWidth { get; private set; } = 0.18f;

        [field: SerializeField]
        public float RouteTransitWidth { get; private set; } = 0.22f;

        internal Color ResolveNodeColor(MapNodePresentationState state)
        {
            if (state.Visibility == MapNodeVisibility.Hidden && state.IsExplorable)
            {
                return NodeExplorable;
            }

            if (state.Visibility == MapNodeVisibility.Present)
            {
                return NodePresent;
            }

            if (state.Visibility == MapNodeVisibility.Hidden)
            {
                return NodeHidden;
            }

            return NodeExplored;
        }

        internal Color ResolveFleetColor(bool isSelected)
        {
            return isSelected ? FleetMarkerSelected : FleetMarker;
        }

        internal Color RouteColor(MapRouteState state)
        {
            switch (state)
            {
                case MapRouteState.Planned:
                    return RoutePlanned;
                case MapRouteState.InTransit:
                    return RouteTransit;
                default:
                    return RouteKnown;
            }
        }

        internal float RouteWidth(MapRouteState state)
        {
            switch (state)
            {
                case MapRouteState.Planned:
                    return RoutePlannedWidth;
                case MapRouteState.InTransit:
                    return RouteTransitWidth;
                default:
                    return RouteKnownWidth;
            }
        }
    }
}
