using System;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// Settles colony production once per second of simulation time, after travel resolution.
    /// Building rates are per second. The active run arrives through <see cref="ISimulationSystem.OnRunChanged"/>.
    /// </summary>
    internal sealed class EconomyTickSystem : ISimulationSystem
    {
        internal const float kIntervalSeconds = 1f;

        private readonly EconomyService _economy;
        private RunState _run;
        private float _elapsedSeconds;

        public EconomyTickSystem(EconomyService economy)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        public void OnRunChanged(RunState run)
        {
            if (ReferenceEquals(run, _run))
            {
                return;
            }

            _run = run;
            ResetStep();
        }

        public void OnTimelineReset()
        {
            ResetStep();
        }

        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            _ = time;
            if (_run == null || tickDeltaSeconds <= 0f)
            {
                return;
            }

            _elapsedSeconds += tickDeltaSeconds;
            while (_elapsedSeconds >= kIntervalSeconds)
            {
                _elapsedSeconds -= kIntervalSeconds;
                _economy.ApplyTick(_run);
            }
        }

        private void ResetStep()
        {
            _elapsedSeconds = 0f;
            _economy.ClearOutputRemainder();
        }
    }
}
