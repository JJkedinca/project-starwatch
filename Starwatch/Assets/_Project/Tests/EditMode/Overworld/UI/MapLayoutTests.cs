using EndlessHeros.Overworld.UI;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for map coordinate conversion.
    /// </summary>
    public sealed class MapLayoutTests
    {
        [Test]
        public void ToLocalPosition_ScalesAndFlipsY()
        {
            var local = MapLayout.ToLocalPosition(
                new Vector2(2f, 1f),
                scale: 10f,
                originOffset: new Vector2(5f, 7f));

            Assert.That(local.x, Is.EqualTo(25f));
            Assert.That(local.y, Is.EqualTo(-3f));
        }

        [Test]
        public void ComputeOriginOffset_KeepsHighestNodeAtTopPadding()
        {
            var positions = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(2f, 1f),
                new Vector2(4f, 2f),
                new Vector2(2f, -1f),
            };
            var padding = new Vector2(48f, 48f);
            const float scale = 72f;

            var origin = MapLayout.ComputeOriginOffset(positions, scale, padding);
            var topNode = MapLayout.ToLocalPosition(new Vector2(4f, 2f), scale, origin);
            var leftNode = MapLayout.ToLocalPosition(new Vector2(0f, 0f), scale, origin);
            var bottomNode = MapLayout.ToLocalPosition(new Vector2(2f, -1f), scale, origin);

            Assert.That(topNode.y, Is.EqualTo(padding.y));
            Assert.That(leftNode.x, Is.EqualTo(padding.x));
            Assert.That(bottomNode.y, Is.GreaterThan(topNode.y));
            Assert.That(topNode.y, Is.GreaterThanOrEqualTo(0f));
        }
    }
}
