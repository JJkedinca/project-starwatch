using System;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Settlement;

namespace EndlessHeros.Research
{
    /// <summary>
    /// Hears run and timeline changes from the simulation loop. Science is taken from
    /// <see cref="EconomyTickSystem"/> after each settled second.
    /// </summary>
    internal sealed class ResearchTickSystem : ISimulationSystem
    {
        private readonly ResearchService _research;
        private readonly EconomyService _economy;
        private RunState _run;

        public ResearchTickSystem(ResearchService research, EconomyService economy, EconomyTickSystem economyTick)
        {
            _research = research ?? throw new ArgumentNullException(nameof(research));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            if (economyTick == null)
            {
                throw new ArgumentNullException(nameof(economyTick));
            }

            economyTick.AfterEachSecond(Consume);
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
            // Science is applied from the economy-second callback, not from this tick.
            _ = time;
            _ = tickDeltaSeconds;
        }

        /// <summary>
        /// Called once after each economy second, including a second that produced zero Science.
        /// </summary>
        public void Consume()
        {
            if (_run == null)
            {
                return;
            }

            _research.ApplyScience(_run, _economy.ProducedScience);
        }
    }
}
