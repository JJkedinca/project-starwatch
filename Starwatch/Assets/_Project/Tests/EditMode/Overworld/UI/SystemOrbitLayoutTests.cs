using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for star-system orbit placement.
    /// </summary>
    public sealed class SystemOrbitLayoutTests
    {
        [Test]
        public void MaxOrbitRadius_KeepsTiltedOrbitInsideScene()
        {
            var size = new Vector2(1000f, 500f);

            var radius = SystemOrbitLayout.MaxOrbitRadius(size);

            Assert.That(radius, Is.LessThanOrEqualTo((size.x * 0.5f) - SystemOrbitLayout.kEdgePadding));
            Assert.That(
                radius * SystemOrbitLayout.kTilt,
                Is.LessThanOrEqualTo((size.y * 0.5f) - SystemOrbitLayout.kEdgePadding + 0.001f));
        }

        [Test]
        public void MaxOrbitRadius_NeverDropsBelowInnerOrbit()
        {
            var radius = SystemOrbitLayout.MaxOrbitRadius(new Vector2(100f, 100f));

            Assert.That(radius, Is.EqualTo(SystemOrbitLayout.kMinOrbitRadius));
        }

        [Test]
        public void OrbitRadius_GrowsFromInnerToOuter()
        {
            const float maxRadius = 400f;

            var inner = SystemOrbitLayout.OrbitRadius(0, 4, maxRadius);
            var second = SystemOrbitLayout.OrbitRadius(1, 4, maxRadius);
            var outer = SystemOrbitLayout.OrbitRadius(3, 4, maxRadius);

            Assert.That(inner, Is.EqualTo(SystemOrbitLayout.kMinOrbitRadius));
            Assert.That(second, Is.GreaterThan(inner));
            Assert.That(outer, Is.EqualTo(maxRadius));
        }

        [Test]
        public void OrbitRadius_SinglePlanetSitsBetweenBounds()
        {
            var radius = SystemOrbitLayout.OrbitRadius(0, 1, 400f);

            Assert.That(radius, Is.GreaterThan(SystemOrbitLayout.kMinOrbitRadius));
            Assert.That(radius, Is.LessThan(400f));
        }

        [Test]
        public void OrbitAngle_IsStableAndInRange()
        {
            var planetId = ContentId.From("home-prime");

            var first = SystemOrbitLayout.OrbitAngle(2, planetId);
            var second = SystemOrbitLayout.OrbitAngle(2, planetId);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.InRange(0f, Mathf.PI * 2f));
        }

        [Test]
        public void OrbitAngle_SpreadsNeighboringPlanets()
        {
            var planetId = ContentId.From("home-prime");

            var a = SystemOrbitLayout.OrbitAngle(0, planetId);
            var b = SystemOrbitLayout.OrbitAngle(1, planetId);
            var gap = Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, b * Mathf.Rad2Deg));

            Assert.That(gap, Is.GreaterThan(90f));
        }

        [Test]
        public void PlanetCenter_SquashesVerticalAxisByTilt()
        {
            var center = new Vector2(500f, 300f);

            var right = SystemOrbitLayout.PlanetCenter(center, 200f, 0f);
            var front = SystemOrbitLayout.PlanetCenter(center, 200f, Mathf.PI * 0.5f);

            Assert.That(right.x, Is.EqualTo(700f).Within(0.001f));
            Assert.That(right.y, Is.EqualTo(300f).Within(0.001f));
            Assert.That(front.x, Is.EqualTo(500f).Within(0.001f));
            Assert.That(front.y, Is.EqualTo(300f + (200f * SystemOrbitLayout.kTilt)).Within(0.001f));
        }

        [Test]
        public void AwayFromStar_PointsFromStarTowardPlanet()
        {
            var center = new Vector2(500f, 300f);
            const float angle = 2.1f;

            var planet = SystemOrbitLayout.PlanetCenter(center, 200f, angle);
            var away = SystemOrbitLayout.AwayFromStar(angle);

            Assert.That(away.magnitude, Is.EqualTo(1f).Within(0.001f));
            Assert.That(Vector2.Dot(away, (planet - center).normalized), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void PlanetSizeAndHue_AreStableAndBounded()
        {
            var planetId = ContentId.From("home-moon");

            var size = SystemOrbitLayout.PlanetSize(planetId);
            var hue = SystemOrbitLayout.HueVariant(planetId);

            Assert.That(size, Is.EqualTo(SystemOrbitLayout.PlanetSize(planetId)));
            Assert.That(size, Is.InRange(SystemOrbitLayout.kMinPlanetSize, SystemOrbitLayout.kMaxPlanetSize));
            Assert.That(hue, Is.EqualTo(SystemOrbitLayout.HueVariant(planetId)));
            Assert.That(hue, Is.InRange(0, SystemOrbitLayout.kHueVariantCount - 1));
        }
    }
}
