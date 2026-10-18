using System;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Settlement
{
    /// <summary>
    /// Settles colony rates every simulation second, then notifies listeners once every <see cref="kIntervalSeconds"/> seconds.
    /// The ledger matches the colonies that existed during each second. The active run arrives through <see cref="ISimulationSystem.OnRunChanged"/>.
    /// </summary>
    internal sealed class EconomyTickSystem : ISimulationSystem
    {
        internal const int kIntervalSeconds = 10;

        private readonly EconomyService _economy;
        private Action<float> _afterSettlement;
        private RunState _run;
        private float _elapsedSeconds;
        private int _pendingSeconds;
        private float _windowScience;
        private bool _windowChanged;

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
        /// Runs when the 10-second window closes. The argument is the Science granted across that window.
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
                _windowChanged |= _economy.SettleSecond(_run);
                _windowScience += _economy.ProducedScience;
                _pendingSeconds++;
                if (_pendingSeconds < kIntervalSeconds)
                {
                    continue;
                }

                _pendingSeconds -= kIntervalSeconds;
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            if (_windowChanged)
            {
                _economy.PublishChanged();
            }

            _afterSettlement?.Invoke(_windowScience);
            _windowScience = 0f;
            _windowChanged = false;
        }

        private void ResetStep()
        {
            _elapsedSeconds = 0f;
            _pendingSeconds = 0;
            _windowScience = 0f;
            _windowChanged = false;
            _economy.ClearOutputRemainder();
        }
    }
}
