using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Gain floaters only animate a single economy-cycle grant.
    /// </summary>
    public sealed class StockpileGainFloaterTests
    {
        [Test]
        public void ShouldAnimateGain_AllowsOneCycleOfGrossProduction()
        {
            Assert.That(StockpileBarView.ShouldAnimateGain(2, grossPerSecond: 2f, cycleSeconds: 1), Is.True);
            Assert.That(StockpileBarView.ShouldAnimateGain(4, grossPerSecond: 2f, cycleSeconds: 2), Is.True);
        }

        [Test]
        public void ShouldAnimateGain_SkipsRewindAndNewRunJumps()
        {
            Assert.That(StockpileBarView.ShouldAnimateGain(500, grossPerSecond: 2f, cycleSeconds: 1), Is.False);
            Assert.That(StockpileBarView.ShouldAnimateGain(0, grossPerSecond: 2f, cycleSeconds: 1), Is.False);
            Assert.That(StockpileBarView.ShouldAnimateGain(-10, grossPerSecond: 2f, cycleSeconds: 1), Is.False);
        }
    }
}
