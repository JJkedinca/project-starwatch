using System;

namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Authoritative simulation clock. Tracks pause/speed and publishes time
    /// advancement without knowing about planets, fleets, or invasions.
    /// </summary>
    public sealed class SimulationClock
    {
        private const float kNormalMultiplier = 1f;
        private const float kFastMultiplier = 2f;
        private const float kFasterMultiplier = 4f;

        private SimulationSpeed _speed = SimulationSpeed.Normal;

        public GameTime CurrentTime { get; private set; } = GameTime.Zero;

        public SimulationSpeed Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public bool IsPaused => _speed == SimulationSpeed.Paused;

        public event Action<GameTime, float> TimeAdvanced;

        /// <summary>
        /// Pauses the clock, or resumes to Normal when currently paused.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (paused)
            {
                _speed = SimulationSpeed.Paused;
                return;
            }

            if (_speed == SimulationSpeed.Paused)
            {
                _speed = SimulationSpeed.Normal;
            }
        }

        /// <summary>
        /// Advances simulation time from a real-time delta using the current speed.
        /// Returns the simulation delta actually applied.
        /// </summary>
        public float Advance(float realDeltaTime)
        {
            if (IsPaused || realDeltaTime <= 0f)
            {
                return 0f;
            }

            var simulationDelta = realDeltaTime * GetSpeedMultiplier(_speed);
            CurrentTime = CurrentTime.Add(simulationDelta);
            TimeAdvanced?.Invoke(CurrentTime, simulationDelta);
            return simulationDelta;
        }

        /// <summary>
        /// Forces one tick of advancement for tests/debug, preserving prior pause state.
        /// </summary>
        public float AdvanceOneTick(float tickDeltaTime)
        {
            if (tickDeltaTime <= 0f)
            {
                return 0f;
            }

            var previousSpeed = _speed;
            _speed = SimulationSpeed.Normal;
            var advanced = Advance(tickDeltaTime);
            _speed = previousSpeed;
            return advanced;
        }

        private static float GetSpeedMultiplier(SimulationSpeed speed)
        {
            switch (speed)
            {
                case SimulationSpeed.Normal:
                    return kNormalMultiplier;
                case SimulationSpeed.Fast:
                    return kFastMultiplier;
                case SimulationSpeed.Faster:
                    return kFasterMultiplier;
                default:
                    return 0f;
            }
        }
    }
}
