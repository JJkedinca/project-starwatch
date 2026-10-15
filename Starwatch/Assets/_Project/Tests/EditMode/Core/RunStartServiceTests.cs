using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EndlessHeros.Core.Bootstrap;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EndlessHeros.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for run-start clock reset and overlapping start guards.
    /// </summary>
    public sealed class RunStartServiceTests
    {
        [Test]
        public async Task StartRunAsync_ResetsClockAndResumesAfterLoad()
        {
            var clock = new SimulationClock();
            clock.SetPaused(false);
            clock.Advance(3f);

            var loader = new FakeSceneLoader();
            var service = CreateService(clock, loader);

            var started = await service.StartRunAsync(
                new RunStartConfiguration("authored-vertical-slice", 7));

            Assert.That(started, Is.True);
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(0d));
            Assert.That(clock.IsPaused, Is.False);
            Assert.That(loader.LoadOverworldCount, Is.EqualTo(1));
        }

        [Test]
        public async Task StartRunAsync_RejectsOverlappingRequests()
        {
            var clock = new SimulationClock();
            var loader = new FakeSceneLoader { BlockOverworldLoad = true };
            var service = CreateService(clock, loader);
            var configuration = new RunStartConfiguration("authored-vertical-slice", 1);

            var first = service.StartRunAsync(configuration);
            await UniTask.Yield();

            Assert.That(service.IsStarting, Is.True);

            var secondStarted = await service.StartRunAsync(configuration);
            Assert.That(secondStarted, Is.False);
            Assert.That(loader.LoadOverworldCount, Is.EqualTo(1));

            loader.ReleaseOverworldLoad();
            var firstStarted = await first;

            Assert.That(firstStarted, Is.True);
            Assert.That(service.IsStarting, Is.False);
            Assert.That(loader.LoadOverworldCount, Is.EqualTo(1));
        }

        [Test]
        public async Task StartRunAsync_DoesNotResetClockWhenCreateFails()
        {
            var clock = new SimulationClock();
            clock.SetPaused(false);
            clock.Advance(3f);
            var loader = new FakeSceneLoader();
            var service = CreateService(clock, loader);
            LogAssert.Expect(
                LogType.Error,
                "RunManager.CreateRun could not find scenario 'missing-scenario'.");

            var started = await service.StartRunAsync(
                new RunStartConfiguration("missing-scenario", 1));

            Assert.That(started, Is.False);
            Assert.That(clock.CurrentTime.Seconds, Is.EqualTo(3d));
            Assert.That(clock.IsPaused, Is.False);
            Assert.That(loader.LoadOverworldCount, Is.EqualTo(0));
        }

        private static RunStartService CreateService(SimulationClock clock, FakeSceneLoader loader)
        {
            return new RunStartService(
                RunTestFixture.CreateRunManager(),
                new SceneFlow(loader),
                clock,
                new SimulationLoop(clock));
        }
    }
}
