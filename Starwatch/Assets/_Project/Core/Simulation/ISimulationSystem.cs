namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Contract for systems stepped by SimulationLoop in a fixed, deterministic order.
    /// Systems must not self-register for Unity Update.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>
        /// Runs one simulation tick after the clock has advanced by tickDeltaSeconds.
        /// </summary>
        void OnSimulationTick(GameTime time, float tickDeltaSeconds);
    }
}
