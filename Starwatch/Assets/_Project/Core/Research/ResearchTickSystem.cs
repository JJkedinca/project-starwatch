using System;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Settlement;

namespace EndlessHeros.Research
{
    /// <summary>
    /// Hears run and timeline changes from the simulation loop. Science is taken from
    /// <see cref="EconomyTickSystem"/> after each 30-second settlement.
    /// </summary>
    internal sealed class ResearchTickSystem : ISimulationSystem
    {
        private readonly ResearchService _research;
        private RunState _run;

        public ResearchTickSystem(ResearchService research, EconomyTickSystem economyTick)
        {
            _research = research ?? throw new ArgumentNullException(nameof(research));
            if (economyTick == null)
            {
                throw new ArgumentNullException(nameof(economyTick));
            }

            economyTick.AfterEachSettlement(Consume);
        }

        public void OnRunChanged(RunState run)
        {
            _run = run;
            _research.OnRunStarted(run);
        }

        public void OnTimelineReset()
        {
            _research.OnRunStarted(_run);
        }

        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            // Science is applied from the economy settlement, not from this tick.
            _ = time;
            _ = tickDeltaSeconds;
        }

        /// <summary>
        /// Called once after each settlement, including a window that produced zero Science.
        /// </summary>
        public void Consume(float science)
        {
            if (_run == null)
            {
                return;
            }

            _research.ApplyScience(_run, science);
        }
    }
}
