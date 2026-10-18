using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Run.Universe;
using EndlessHeros.Core.Simulation;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for fixed-rate SimulationLoop ordering and catch-up.
    /// </summary>
    public sealed class SimulationLoopTests
    {
        private sealed class CountingSystem : ISimulationSystem
        {
            public int Ticks { get; private set; }

            public float LastDelta { get; private set; }

            public RunState Run { get; private set; }

            public int TimelineResets { get; private set; }

            public void OnRunChanged(RunState run)
            {
                Run = run;
            }

            public void OnTimelineReset()
            {
                TimelineResets++;
            }

            public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
            {
                Ticks++;
                LastDelta = tickDeltaSeconds;
            }
        }

        [Test]
        public void Pump_WhenPaused_DoesNotTick()
        {
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var system = new CountingSystem();
            loop.Register(system);

            var ticks = loop.Pump(1f);

            Assert.That(ticks, Is.EqualTo(0));
            Assert.That(system.Ticks, Is.EqualTo(0));
        }

        [Test]
        public void Pump_RunsFixedIntervalTicksInOrder()
        {
            var clock = new SimulationClock();
            clock.SetPaused(false);
            var loop = new SimulationLoop(clock);
            var first = new CountingSystem();
            var second = new CountingSystem();
            loop.Register(first);
            loop.Register(second);

            var ticks = loop.Pump(SimulationLoop.kTickIntervalSeconds * 3f);

            Assert.That(ticks, Is.EqualTo(3));
            Assert.That(first.Ticks, Is.EqualTo(3));
            Assert.That(second.Ticks, Is.EqualTo(3));
            Assert.That(first.LastDelta, Is.EqualTo(SimulationLoop.kTickIntervalSeconds).Within(0.0001f));
            Assert.That(
                clock.CurrentTime.Seconds,
                Is.EqualTo(SimulationLoop.kTickIntervalSeconds * 3d).Within(0.0001d));
        }

        [Test]
        public void Pump_CapsCatchUpTicks()
        {
            var clock = new SimulationClock();
            clock.SetPaused(false);
            var loop = new SimulationLoop(clock);
            var system = new CountingSystem();
            loop.Register(system);

            var ticks = loop.Pump(10f);

            Assert.That(ticks, Is.EqualTo(SimulationLoop.kMaxCatchUpTicks));
            Assert.That(system.Ticks, Is.EqualTo(SimulationLoop.kMaxCatchUpTicks));
        }

        [Test]
        public void NotifyRunChanged_TellsEveryRegisteredSystem()
        {
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var first = new CountingSystem();
            var second = new CountingSystem();
            loop.Register(first);
            loop.Register(second);
            var run = RunTestFixture.CreateRunManager().CreateRun("authored-vertical-slice", RunSeed.FromValue(9));

            loop.NotifyRunChanged(run);
            var late = new CountingSystem();
            loop.Register(late);

            Assert.That(first.Run, Is.SameAs(run));
            Assert.That(second.Run, Is.SameAs(run));
            Assert.That(late.Run, Is.SameAs(run));

            loop.NotifyRunChanged(null);

            Assert.That(first.Run, Is.Null);
            Assert.That(second.Run, Is.Null);
            Assert.That(late.Run, Is.Null);
        }

        [Test]
        public void RecreateTimeline_NotifiesRegisteredSystems()
        {
            var clock = new SimulationClock();
            var loop = new SimulationLoop(clock);
            var system = new CountingSystem();
            loop.Register(system);
            var database = RunTestFixture.CreateObjectDatabase();
            var timelineFactory = new TimelineFactory();
            var manager = new RunManager(
                timelineFactory,
                new UniverseLoader(timelineFactory),
                database,
                new EventJournal(),
                loop);
            var run = manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(10));

            var timeline = manager.RecreateTimeline(1);

            Assert.That(timeline, Is.Not.Null);
            Assert.That(run.Timeline, Is.SameAs(timeline));
            Assert.That(system.Run, Is.SameAs(run));
            Assert.That(system.TimelineResets, Is.EqualTo(1));
        }
    }
}
