using System;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// Accumulates per-second colony rates and settles them every 30 seconds of simulation time.
    /// The stockpile and research hear one result for the whole window. The active run arrives through <see cref="ISimulationSystem.OnRunChanged"/>.
    /// </summary>
    internal sealed class EconomyTickSystem : ISimulationSystem
    {
        internal const float kIntervalSeconds = 10f;

        private readonly EconomyService _economy;
        private Action<float> _afterSettlement;
        private RunState _run;
        private float _elapsedSeconds;
        private int _pendingSeconds;

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

        /// <summary>
        /// Runs after each 30-second settlement. The argument is the Science granted across that window.
        /// </summary>
        internal void AfterEachSettlement(Action<float> callback)
        {
            _afterSettlement = callback;
        }

        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            _ = time;
            if (_run == null || tickDeltaSeconds <= 0f)
            {
                return;
            }

            _elapsedSeconds += tickDeltaSeconds;
            while (_elapsedSeconds >= 1f)
            {
                _elapsedSeconds -= 1f;
                _pendingSeconds++;
            }

            var window = (int)kIntervalSeconds;
            while (_pendingSeconds >= window)
            {
                _pendingSeconds -= window;
                SettleWindow(window);
            }
        }

        private void SettleWindow(int seconds)
        {
            var science = 0f;
            var changed = false;
            for (var i = 0; i < seconds; i++)
            {
                changed |= _economy.SettleSecond(_run);
                science += _economy.ProducedScience;
            }

            if (changed)
            {
                _economy.PublishChanged();
            }

            _afterSettlement?.Invoke(science);
        }

        private void ResetStep()
        {
            _elapsedSeconds = 0f;
            _pendingSeconds = 0;
            _economy.ClearOutputRemainder();
        }
    }
}
