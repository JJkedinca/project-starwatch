using System;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Settlement
{
    /// <summary>
    /// Settles colony rates every simulation second. Science is handed to listeners each second;
    /// resource-changed notifications batch once every <see cref="kIntervalSeconds"/> seconds.
    /// The ledger matches the colonies that existed during each second. The active run arrives through <see cref="ISimulationSystem.OnRunChanged"/>.
    /// </summary>
    internal sealed class EconomyTickSystem : ISimulationSystem
    {
        internal const int kIntervalSeconds = 10;

        private readonly EconomyService _economyService;
        private Action<float> _afterSettlement;
        private RunState _run;
        private float _elapsedSeconds;
        private int _pendingSeconds;
        private bool _windowChanged;

        public EconomyTickSystem(EconomyService economyService)
        {
            _economyService = economyService ?? throw new ArgumentNullException(nameof(economyService));
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
        /// Runs after each settled second. The argument is that second's Science output.
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
                _windowChanged |= _economyService.SettleSecond(_run);
                // Science every second so research progress moves with the clock.
                _afterSettlement?.Invoke(_economyService.ProducedScience);
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
            var changed = _windowChanged;
            _windowChanged = false;
            if (changed)
            {
                _economyService.PublishChanged();
            }
        }

        private void ResetStep()
        {
            _elapsedSeconds = 0f;
            _pendingSeconds = 0;
            _windowChanged = false;
            _economyService.ClearOutputRemainder();
        }
    }
}
