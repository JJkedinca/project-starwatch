using ProjectStarwatch.Core.Run;

namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Contract for systems stepped by SimulationLoop in a fixed, deterministic order.
    /// Systems must not self-register for Unity Update.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>
        /// Receives the active run when it is created, replaced, or cleared.
        /// </summary>
        void OnRunChanged(RunState run);

        /// <summary>
        /// Drops timeline-local tick state after the active run replaces its timeline.
        /// </summary>
        void OnTimelineReset();

        /// <summary>
        /// Runs one simulation tick after the clock has advanced by tickDeltaSeconds.
        /// </summary>
        void OnSimulationTick(GameTime time, float tickDeltaSeconds);
    }
}
