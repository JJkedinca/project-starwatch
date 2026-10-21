using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Once-a-second transit ETA refresh gate.
    /// </summary>
    public sealed class TransitCountdownTests
    {
        [Test]
        public void ShouldRefresh_OnlyWhileTravelingAndOncePerSecond()
        {
            var clock = new SimulationClock();
            var countdown = new TransitCountdown();

            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 1f), Is.False);

            countdown.SetTraveling(true, clock);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 1f), Is.False);

            clock.AdvanceSimulation(1f);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 1f), Is.True);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 1f), Is.False);

            clock.AdvanceSimulation(0.5f);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 0.5f), Is.False);

            clock.AdvanceSimulation(0.5f);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 0.5f), Is.True);

            countdown.Clear();
            clock.AdvanceSimulation(1f);
            Assert.That(countdown.ShouldRefresh(clock.CurrentTime, 1f), Is.False);
        }
    }
}
