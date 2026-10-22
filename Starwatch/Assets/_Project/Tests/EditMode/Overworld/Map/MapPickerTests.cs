using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class MapPickerTests
    {
        [Test]
        public void TryPickNearest_SelectsClosestWithinRadius()
        {
            var nodes = new List<MapPicker.NodePickPoint>
            {
                new(new ContentId("a"), new Vector3(0f, 0f, 0f)),
                new(new ContentId("b"), new Vector3(5f, 0f, 0f)),
            };
            var ray = new Ray(new Vector3(0.4f, 10f, 0f), Vector3.down);

            var hit = MapPicker.TryPickNearest(ray, nodes, pickRadius: 2f, out var id);

            Assert.That(hit, Is.True);
            Assert.That(id.Value, Is.EqualTo("a"));
        }

        [Test]
        public void TryPickNearest_MissesOutsideRadius()
        {
            var nodes = new List<MapPicker.NodePickPoint>
            {
                new(new ContentId("a"), new Vector3(0f, 0f, 0f)),
            };
            var ray = new Ray(new Vector3(10f, 10f, 0f), Vector3.down);

            var hit = MapPicker.TryPickNearest(ray, nodes, pickRadius: 2f, out var id);

            Assert.That(hit, Is.False);
            Assert.That(id.IsEmpty, Is.True);
        }

        [Test]
        public void TryPickNearest_BreaksTiesByContentId()
        {
            var nodes = new List<MapPicker.NodePickPoint>
            {
                new(new ContentId("b"), new Vector3(1f, 0f, 0f)),
                new(new ContentId("a"), new Vector3(1f, 0f, 0f)),
            };
            var ray = new Ray(new Vector3(1f, 10f, 0f), Vector3.down);

            var hit = MapPicker.TryPickNearest(ray, nodes, pickRadius: 2f, out var id);

            Assert.That(hit, Is.True);
            Assert.That(id.Value, Is.EqualTo("a"));
        }
    }
}
