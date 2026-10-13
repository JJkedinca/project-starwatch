namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Legacy alias for simulation systems. Prefer ISimulationSystem.
    /// TODO: Remove once all callers use ISimulationSystem.
    /// </summary>
    public interface ITickable : ISimulationSystem
    {
    }
}
