using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class FleetTransitPlacementTests
    {
        [Test]
        public void Progress_AtStartMiddleAndEnd()
        {
            Assert.That(FleetTransitPlacement.Progress(10f, 10f), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(FleetTransitPlacement.Progress(5f, 10f), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(FleetTransitPlacement.Progress(0f, 10f), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Evaluate_LerpsAlongRoute()
        {
            var from = new Vector3(0f, 0f, 0f);
            var to = new Vector3(10f, 0f, 0f);
            var mid = FleetTransitPlacement.Evaluate(from, to, remainingSeconds: 5f, totalSeconds: 10f);

            Assert.That(mid.x, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void Progress_DoesNotAdvanceWhenRemainingUnchanged()
        {
            var from = new Vector3(0f, 0f, 0f);
            var to = new Vector3(10f, 0f, 0f);
            var first = FleetTransitPlacement.Evaluate(from, to, 4f, 8f);
            var second = FleetTransitPlacement.Evaluate(from, to, 4f, 8f);

            Assert.That(first, Is.EqualTo(second));
        }
    }
}
