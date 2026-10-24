using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    /// <summary>
    /// EditMode coverage for UpdateManager frame ticks and simulation pump order.
    /// </summary>
    public sealed class UpdateManagerTests
    {
        private sealed class CountingFrameTickable : IFrameTickable
        {
            public int Calls { get; private set; }

            public float LastUnscaledDelta { get; private set; }

            public void ManagedUpdate(float unscaledDeltaTime)
            {
                Calls++;
                LastUnscaledDelta = unscaledDeltaTime;
            }
        }

        private sealed class CountingSimulation : ISimulationUpdateManager
        {
            public int PumpCalls { get; private set; }

            public float LastRealDelta { get; private set; }

            public int RegisterCalls { get; private set; }

            public void Register(ISimulationSystem system)
            {
                RegisterCalls++;
            }

            public void Unregister(ISimulationSystem system)
            {
            }

            public void NotifyRunChanged(RunState run)
            {
            }

            public void NotifyTimelineReset()
            {
            }

            public void ResetAccumulator()
            {
            }

            public int Pump(float realDeltaTime)
            {
                PumpCalls++;
                LastRealDelta = realDeltaTime;
                return 0;
            }
        }

        private sealed class UnregisteringFrameTickable : IFrameTickable
        {
            private readonly IUpdateManager _updateManager;

            public int Calls { get; private set; }

            public UnregisteringFrameTickable(IUpdateManager updateManager)
            {
                _updateManager = updateManager;
            }

            public void ManagedUpdate(float unscaledDeltaTime)
            {
                Calls++;
                _updateManager.Unregister(this);
            }
        }

        private sealed class UnregisterOtherFrameTickable : IFrameTickable
        {
            private readonly IUpdateManager _updateManager;
            private readonly IFrameTickable _other;

            public int Calls { get; private set; }

            public UnregisterOtherFrameTickable(IUpdateManager updateManager, IFrameTickable other)
            {
                _updateManager = updateManager;
                _other = other;
            }

            public void ManagedUpdate(float unscaledDeltaTime)
            {
                Calls++;
                _updateManager.Unregister(_other);
            }
        }

        [Test]
        public void Tick_InvokesFrameTickablesThenPumpsSimulation()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var simulation = new CountingSimulation();
            var frame = new CountingFrameTickable();
            manager.Configure(simulation);
            manager.Register(frame);

            manager.Tick(0.016f, 0.02f);

            Assert.That(frame.Calls, Is.EqualTo(1));
            Assert.That(frame.LastUnscaledDelta, Is.EqualTo(0.016f).Within(0.0001f));
            Assert.That(simulation.PumpCalls, Is.EqualTo(1));
            Assert.That(simulation.LastRealDelta, Is.EqualTo(0.02f).Within(0.0001f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_AfterUnregister_DoesNotInvokeFrameTickable()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var simulation = new CountingSimulation();
            var frame = new CountingFrameTickable();
            manager.Configure(simulation);
            manager.Register(frame);
            manager.Unregister(frame);

            manager.Tick(0.016f, 0.02f);

            Assert.That(frame.Calls, Is.EqualTo(0));
            Assert.That(simulation.PumpCalls, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_WhenFrameTickableUnregistersItself_DoesNotThrow()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var simulation = new CountingSimulation();
            var frame = new UnregisteringFrameTickable(manager);
            manager.Configure(simulation);
            manager.Register(frame);

            Assert.DoesNotThrow(() => manager.Tick(0.016f, 0.02f));
            Assert.That(frame.Calls, Is.EqualTo(1));

            manager.Tick(0.016f, 0.02f);
            Assert.That(frame.Calls, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_WhenTickableUnregistersEarlierEntry_DoesNotDoubleTickRemover()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var simulation = new CountingSimulation();
            var earlier = new CountingFrameTickable();
            var remover = new UnregisterOtherFrameTickable(manager, earlier);
            manager.Configure(simulation);
            manager.Register(earlier);
            manager.Register(remover);

            manager.Tick(0.016f, 0.02f);

            Assert.That(earlier.Calls, Is.EqualTo(1));
            Assert.That(remover.Calls, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_WhenTickableUnregistersLaterEntry_SkipsUnregistered()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var simulation = new CountingSimulation();
            var later = new CountingFrameTickable();
            var remover = new UnregisterOtherFrameTickable(manager, later);
            manager.Configure(simulation);
            manager.Register(remover);
            manager.Register(later);

            manager.Tick(0.016f, 0.02f);

            Assert.That(remover.Calls, Is.EqualTo(1));
            Assert.That(later.Calls, Is.EqualTo(0));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_WithoutSimulation_StillTicksFrameConsumers()
        {
            var go = new GameObject("update-manager-test");
            var manager = go.AddComponent<UpdateManager>();
            var frame = new CountingFrameTickable();
            manager.Register(frame);

            Assert.DoesNotThrow(() => manager.Tick(0.01f, 0.01f));
            Assert.That(frame.Calls, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }
    }
}
