using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class MapCameraBoundsTests
    {
        [Test]
        public void ClampCameraPosition_KeepsInsideBox()
        {
            var bounds = new MapCameraBounds(new Vector2(-10f, -5f), new Vector2(10f, 5f));
            var clamped = bounds.ClampCameraPosition(new Vector3(40f, 20f, -40f));

            Assert.That(clamped.x, Is.EqualTo(10f));
            Assert.That(clamped.y, Is.EqualTo(20f));
            Assert.That(clamped.z, Is.EqualTo(-5f));
        }

        [Test]
        public void FromWorldPoints_AppliesMargin()
        {
            var bounds = MapCameraBounds.FromWorldPoints(
                new[] { new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 4f) },
                margin: 2f);

            Assert.That(bounds.Min.x, Is.EqualTo(-2f));
            Assert.That(bounds.Max.x, Is.EqualTo(12f));
            Assert.That(bounds.Min.y, Is.EqualTo(-2f));
            Assert.That(bounds.Max.y, Is.EqualTo(6f));
        }

        [Test]
        public void FromWorldPoints_EmptyList_IsEmpty()
        {
            var bounds = MapCameraBounds.FromWorldPoints(System.Array.Empty<Vector3>(), 1f);
            Assert.That(bounds.IsEmpty, Is.True);
        }

        [Test]
        public void Empty_DoesNotClampCamera()
        {
            var cameraPosition = new Vector3(12f, 20f, -8f);
            var clamped = MapCameraBounds.Empty.ClampCameraPosition(cameraPosition);

            Assert.That(clamped, Is.EqualTo(cameraPosition));
        }
    }
}
