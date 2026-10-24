using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Map;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class StarSystemPresentationSOTests
    {
        [Test]
        public void ResolvePlanetColor_UsesSelectedOverScanned()
        {
            var presentation = ScriptableObject.CreateInstance<StarSystemPresentationSO>();

            var selected = presentation.ResolvePlanetColor(
                new PlanetPresentationState(PlanetVisibility.Scanned, true, "A"));
            var scanned = presentation.ResolvePlanetColor(
                new PlanetPresentationState(PlanetVisibility.Scanned, false, "A"));
            var unscanned = presentation.ResolvePlanetColor(
                new PlanetPresentationState(PlanetVisibility.SystemExplored, false, "A"));

            Assert.That(selected, Is.EqualTo(presentation.PlanetSelected));
            Assert.That(scanned, Is.EqualTo(presentation.PlanetScanned));
            Assert.That(unscanned, Is.EqualTo(presentation.PlanetUnscanned));

            Object.DestroyImmediate(presentation);
        }

        [Test]
        public void ResolveOrbit_UsesSelectedWidthsAndColors()
        {
            var presentation = ScriptableObject.CreateInstance<StarSystemPresentationSO>();

            Assert.That(presentation.ResolveOrbitColor(true), Is.EqualTo(presentation.OrbitRingSelected));
            Assert.That(presentation.ResolveOrbitWidth(true), Is.EqualTo(presentation.SelectedOrbitRingWidth));
            Assert.That(presentation.ResolveOrbitColor(false), Is.EqualTo(presentation.OrbitRing));
            Assert.That(presentation.ResolveOrbitWidth(false), Is.EqualTo(presentation.OrbitRingWidth));

            Object.DestroyImmediate(presentation);
        }
    }
}
