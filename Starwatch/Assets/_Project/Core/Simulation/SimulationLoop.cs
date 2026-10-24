using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Run;

namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Fixed-rate simulation driver. Accumulates real time, advances the clock in
    /// discrete ticks, then runs registered systems in registration order.
    /// </summary>
    internal sealed class SimulationLoop : ISimulationUpdateManager
    {
        public SimulationClock Clock { get; }

        public int SystemCount => _systems.Count;

        internal const float kTickRateHz = 30f;
        internal const int kMaxCatchUpTicks = 10;
        internal const float kTickIntervalSeconds = 1f / kTickRateHz;

        private readonly List<ISimulationSystem> _systems = new();
        private RunState _run;
        private float _accumulator;

        public SimulationLoop(SimulationClock clock)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Adds a system to the end of the deterministic tick order.
        /// </summary>
        public void Register(ISimulationSystem system)
        {
            if (system == null)
            {
                return;
            }

            if (_systems.Contains(system))
            {
                return;
            }

            _systems.Add(system);
            system.OnRunChanged(_run);
        }

        /// <summary>
        /// Removes a previously registered system.
        /// </summary>
        public void Unregister(ISimulationSystem system)
        {
            if (system == null)
            {
                return;
            }

            _systems.Remove(system);
        }

        /// <summary>
        /// Stores the active run and tells every registered system. Null clears it.
        /// A system registered later receives this same run.
        /// </summary>
        public void NotifyRunChanged(RunState run)
        {
            _run = run;
            for (var i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnRunChanged(run);
            }
        }

        /// <summary>
        /// Tells every registered system that the active run replaced its timeline.
        /// </summary>
        public void NotifyTimelineReset()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnTimelineReset();
            }
        }

        /// <summary>
        /// Clears leftover frame accumulation (call with clock reset on new runs).
        /// </summary>
        public void ResetAccumulator()
        {
            _accumulator = 0f;
        }

        /// <summary>
        /// Pumps real-time delta into fixed simulation ticks. Returns ticks executed.
        /// </summary>
        public int Pump(float realDeltaTime)
        {
            if (Clock.IsPaused || realDeltaTime <= 0f)
            {
                return 0;
            }

            _accumulator += realDeltaTime * GetSpeedMultiplier(Clock.Speed);
            var ticks = 0;

            while (_accumulator >= kTickIntervalSeconds && ticks < kMaxCatchUpTicks)
            {
                _accumulator -= kTickIntervalSeconds;
                Clock.AdvanceSimulation(kTickIntervalSeconds);

                var time = Clock.CurrentTime;
                for (var i = 0; i < _systems.Count; i++)
                {
                    _systems[i].OnSimulationTick(time, kTickIntervalSeconds);
                }

                ticks++;
            }

            if (_accumulator > kTickIntervalSeconds * kMaxCatchUpTicks)
            {
                _accumulator = kTickIntervalSeconds * kMaxCatchUpTicks;
            }

            return ticks;
        }

        private static float GetSpeedMultiplier(SimulationSpeed speed)
        {
            switch (speed)
            {
                case SimulationSpeed.Normal:
                    return 1f;
                case SimulationSpeed.Fast:
                    return 2f;
                case SimulationSpeed.Faster:
                    return 4f;
                default:
                    return 0f;
            }
        }
    }
}
