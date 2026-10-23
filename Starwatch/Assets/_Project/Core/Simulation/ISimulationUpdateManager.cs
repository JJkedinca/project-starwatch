using ProjectStarwatch.Core.Run;

namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Fixed-rate simulation driver: registers systems and pumps real time into discrete ticks.
    /// </summary>
    public interface ISimulationUpdateManager
    {
        void Register(ISimulationSystem system);

        void Unregister(ISimulationSystem system);

        void NotifyRunChanged(RunState run);

        void NotifyTimelineReset();

        void ResetAccumulator();

        int Pump(float realDeltaTime);
    }
}
