using NUnit.Framework;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Map;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class StarMapPresentationSOTests
    {
        [Test]
        public void ResolveNodeColor_UsesExplorableWhenHiddenNeighbor()
        {
            var presentation = ScriptableObject.CreateInstance<StarMapPresentationSO>();
            var state = new MapNodePresentationState(
                MapNodeVisibility.Hidden,
                isExplorable: true,
                isSelected: false,
                hasFleet: false,
                isFleetTraveling: false,
                revealedLabel: "Edge");

            Assert.That(presentation.ResolveNodeColor(state), Is.EqualTo(presentation.NodeExplorable));
            Object.DestroyImmediate(presentation);
        }

        [Test]
        public void RouteColorAndWidth_MatchState()
        {
            var presentation = ScriptableObject.CreateInstance<StarMapPresentationSO>();

            Assert.That(presentation.RouteColor(MapRouteState.Known), Is.EqualTo(presentation.RouteKnown));
            Assert.That(presentation.RouteColor(MapRouteState.Planned), Is.EqualTo(presentation.RoutePlanned));
            Assert.That(presentation.RouteColor(MapRouteState.InTransit), Is.EqualTo(presentation.RouteTransit));
            Assert.That(presentation.RouteWidth(MapRouteState.Known), Is.EqualTo(presentation.RouteKnownWidth));
            Assert.That(presentation.RouteWidth(MapRouteState.Planned), Is.EqualTo(presentation.RoutePlannedWidth));
            Assert.That(presentation.RouteWidth(MapRouteState.InTransit), Is.EqualTo(presentation.RouteTransitWidth));

            Object.DestroyImmediate(presentation);
        }

        [Test]
        public void ResolveFleetColor_UsesSelectedTint()
        {
            var presentation = ScriptableObject.CreateInstance<StarMapPresentationSO>();

            Assert.That(presentation.ResolveFleetColor(false), Is.EqualTo(presentation.FleetMarker));
            Assert.That(presentation.ResolveFleetColor(true), Is.EqualTo(presentation.FleetMarkerSelected));

            Object.DestroyImmediate(presentation);
        }
    }
}
