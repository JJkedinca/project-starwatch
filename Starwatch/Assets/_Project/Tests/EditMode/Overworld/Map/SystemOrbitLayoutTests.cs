using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.Map;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    /// <summary>
    /// EditMode coverage for world-unit star-system orbit placement.
    /// </summary>
    public sealed class SystemOrbitLayoutTests
    {
        [Test]
        public void OrbitRadius_GrowsFromInnerToOuter()
        {
            const float minRadius = 4f;
            const float maxRadius = 12f;

            var inner = SystemOrbitLayout.OrbitRadius(0, 4, minRadius, maxRadius);
            var second = SystemOrbitLayout.OrbitRadius(1, 4, minRadius, maxRadius);
            var outer = SystemOrbitLayout.OrbitRadius(3, 4, minRadius, maxRadius);

            Assert.That(inner, Is.EqualTo(minRadius));
            Assert.That(second, Is.GreaterThan(inner));
            Assert.That(outer, Is.EqualTo(maxRadius));
        }

        [Test]
        public void OrbitRadius_SinglePlanetSitsBetweenBounds()
        {
            var radius = SystemOrbitLayout.OrbitRadius(0, 1, 4f, 12f);

            Assert.That(radius, Is.GreaterThan(4f));
            Assert.That(radius, Is.LessThan(12f));
        }

        [Test]
        public void OrbitAngle_IsStableAndInRange()
        {
            var planetId = ContentId.From("orbit-layout-planet-a");

            var first = SystemOrbitLayout.OrbitAngle(2, planetId);
            var second = SystemOrbitLayout.OrbitAngle(2, planetId);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.InRange(0f, Mathf.PI * 2f));
        }

        [Test]
        public void OrbitAngle_SpreadsNeighboringPlanets()
        {
            var planetId = ContentId.From("orbit-layout-planet-a");

            var a = SystemOrbitLayout.OrbitAngle(0, planetId);
            var b = SystemOrbitLayout.OrbitAngle(1, planetId);
            var gap = Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, b * Mathf.Rad2Deg));

            Assert.That(gap, Is.GreaterThan(90f));
        }

        [Test]
        public void PlanetWorld_LiesOnCircularOrbitInXZ()
        {
            var star = new Vector3(10f, 0f, -5f);
            const float radius = 8f;
            const float angle = 0f;

            var planet = SystemOrbitLayout.PlanetWorld(star, radius, angle);

            Assert.That(planet.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(planet.x, Is.EqualTo(star.x + radius).Within(0.0001f));
            Assert.That(planet.z, Is.EqualTo(star.z).Within(0.0001f));
            Assert.That(Vector3.Distance(new Vector3(planet.x, 0f, planet.z), new Vector3(star.x, 0f, star.z)),
                Is.EqualTo(radius).Within(0.0001f));
        }

        [Test]
        public void PlanetScale_IsStableAndBounded()
        {
            var run = RunTestFixture.CreateStandardRun(RunTestFixture.CreateRunManager(), 42);
            var planetId = RunTestFixture.SecondPlanetAt(run, RunTestFixture.HomeSystem(run));
            Assert.That(planetId.IsEmpty, Is.False);
            const float minScale = 0.45f;
            const float maxScale = 0.85f;

            var scale = SystemOrbitLayout.PlanetScale(planetId, minScale, maxScale);

            Assert.That(scale, Is.EqualTo(SystemOrbitLayout.PlanetScale(planetId, minScale, maxScale)));
            Assert.That(scale, Is.InRange(minScale, maxScale));
        }
    }
}
