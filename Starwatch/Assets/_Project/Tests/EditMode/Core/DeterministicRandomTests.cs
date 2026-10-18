using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for seeded deterministic randomness.
    /// </summary>
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var first = new DeterministicRandom(123);
            var second = new DeterministicRandom(123);

            Assert.That(first.Next(0, 100), Is.EqualTo(second.Next(0, 100)));
            Assert.That(first.NextFloat(), Is.EqualTo(second.NextFloat()));
            Assert.That(first.Next(5, 50), Is.EqualTo(second.Next(5, 50)));
        }

        [Test]
        public void DifferentSeeds_CanDiverge()
        {
            var first = new DeterministicRandom(1);
            var second = new DeterministicRandom(2);

            var matched = true;
            for (var i = 0; i < 8; i++)
            {
                if (first.Next(0, 1000) != second.Next(0, 1000))
                {
                    matched = false;
                    break;
                }
            }

            Assert.That(matched, Is.False);
        }
    }
}
