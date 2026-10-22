using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class MapSpaceTests
    {
        [Test]
        public void ToWorld_ScalesOntoMapPlane()
        {
            var world = MapSpace.ToWorld(new Vector2(2f, -1.5f));

            Assert.That(world.x, Is.EqualTo(20f));
            Assert.That(world.y, Is.EqualTo(0f));
            Assert.That(world.z, Is.EqualTo(-15f));
        }

        [Test]
        public void RoundTrip_PreservesMapPosition()
        {
            var map = new Vector2(3.25f, 4.5f);
            var back = MapSpace.ToMap(MapSpace.ToWorld(map));

            Assert.That(back.x, Is.EqualTo(map.x).Within(0.0001f));
            Assert.That(back.y, Is.EqualTo(map.y).Within(0.0001f));
        }
    }
}
