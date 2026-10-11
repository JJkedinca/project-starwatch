using EndlessHeros.Core.Run;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for RunManager run-creation contracts.
    /// </summary>
    public sealed class RunManagerTests
    {
        [Test]
        public void CreateRun_CreatesScenarioTimelineArchiveAndSeed()
        {
            var manager = new RunManager(new TimelineFactory());
            var seed = RunSeed.FromValue(42);

            var run = manager.CreateRun("authored-vertical-slice", seed);

            Assert.That(run, Is.Not.Null);
            Assert.That(manager.HasActiveRun, Is.True);
            Assert.That(run.Seed.Value, Is.EqualTo(42));
            Assert.That(run.Scenario.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(run.Timeline, Is.Not.Null);
            Assert.That(run.Archive, Is.Not.Null);
            Assert.That(run.RewindIndex, Is.EqualTo(0));
        }

        [Test]
        public void CreateRun_RejectsMissingScenarioId()
        {
            var manager = new RunManager(new TimelineFactory());

            var run = manager.CreateRun(" ", RunSeed.FromValue(1));

            Assert.That(run, Is.Null);
            Assert.That(manager.HasActiveRun, Is.False);
        }
    }
}
