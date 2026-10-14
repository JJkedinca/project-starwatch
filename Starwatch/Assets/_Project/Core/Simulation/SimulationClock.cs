using System;

namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Authoritative simulation clock. Tracks pause/speed and simulation time.
    /// Time advances only through AdvanceSimulation (normally via SimulationLoop).
    /// Starts paused so menu/bootstrap time does not count toward a run.
    /// </summary>
    public sealed class SimulationClock
    {
        public GameTime CurrentTime { get; private set; } = GameTime.Zero;

        public SimulationSpeed Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public bool IsPaused => _speed == SimulationSpeed.Paused;

        public event Action<GameTime, float> TimeAdvanced;

        private SimulationSpeed _speed = SimulationSpeed.Paused;

        /// <summary>
        /// Clears elapsed simulation time and leaves the clock paused. Callers
        /// resume when gameplay for the new run should begin.
        /// </summary>
        public void ResetForRun()
        {
            CurrentTime = GameTime.Zero;
            _speed = SimulationSpeed.Paused;
        }

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
        /// Advances simulation time by an already-scaled simulation delta.
        /// Used by SimulationLoop fixed ticks and EditMode tests.
        /// </summary>
        public float AdvanceSimulation(float simulationDeltaSeconds)
        {
            if (simulationDeltaSeconds <= 0f)
            {
                return 0f;
            }

            CurrentTime = CurrentTime.Add(simulationDeltaSeconds);
            TimeAdvanced?.Invoke(CurrentTime, simulationDeltaSeconds);
            return simulationDeltaSeconds;
        }

        /// <summary>
        /// Advances simulation time when not paused. Prefer SimulationLoop.Pump in gameplay.
        /// </summary>
        public float Advance(float simulationDeltaSeconds)
        {
            if (IsPaused)
            {
                return 0f;
            }

            return AdvanceSimulation(simulationDeltaSeconds);
        }

        /// <summary>
        /// Forces one advancement for tests/debug, preserving prior pause state.
        /// </summary>
        public float AdvanceOneTick(float tickDeltaTime)
        {
            if (tickDeltaTime <= 0f)
            {
                return 0f;
            }

            return AdvanceSimulation(tickDeltaTime);
        }
    }
}
