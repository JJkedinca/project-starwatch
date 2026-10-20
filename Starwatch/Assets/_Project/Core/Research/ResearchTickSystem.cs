using System;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Research
{
    /// <summary>
    /// Hears run and timeline changes from the simulation loop. Science is taken from
    /// <see cref="EconomyTickSystem"/> once when each 10-second window closes.
    /// </summary>
    internal sealed class ResearchTickSystem : ISimulationSystem
    {
        private readonly ResearchService _researchService;
        private RunState _run;

        public ResearchTickSystem(ResearchService researchService, EconomyTickSystem economyTick)
        {
            _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
            if (economyTick == null)
            {
                throw new ArgumentNullException(nameof(economyTick));
            }

            economyTick.AfterEachSettlement(Consume);
        }

        public void OnRunChanged(RunState run)
        {
            _run = run;
            _researchService.OnRunStarted(run);
        }

        public void OnTimelineReset()
        {
            _researchService.OnRunStarted(_run);
        }

        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            // Science is applied from the economy settlement, not from this tick.
            _ = time;
            _ = tickDeltaSeconds;
        }

        /// <summary>
        /// Called once when the 10-second window closes, including a window that produced zero Science.
        /// </summary>
        public void Consume(float science)
        {
            if (_run == null)
            {
                return;
            }

            _researchService.ApplyScience(_run, science);
        }
    }
}
