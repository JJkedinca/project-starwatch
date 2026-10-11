using EndlessHeros.Core.Simulation;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for SimulationClock pause and deterministic advancement.
    /// </summary>
    public sealed class SimulationClockTests
    {
        [Test]
        public void Advance_WhenPaused_DoesNotChangeTime()
        {
            var clock = new SimulationClock();
            clock.SetPaused(true);

            var advanced = clock.Advance(1f);

            Assert.That(advanced, Is.EqualTo(0f));
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(0d));
        }

        [Test]
        public void Advance_WhenNormal_AdvancesDeterministically()
        {
            var clock = new SimulationClock();

            var advanced = clock.Advance(0.5f);

            Assert.That(advanced, Is.EqualTo(0.5f));
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(0.5d));
        }

        [Test]
        public void AdvanceOneTick_AdvancesEvenIfPreviouslyPaused()
        {
            var clock = new SimulationClock();
            clock.SetPaused(true);

            var advanced = clock.AdvanceOneTick(1f);

            Assert.That(advanced, Is.EqualTo(1f));
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(1d));
            Assert.That(clock.IsPaused, Is.True);
        }
    }
}
