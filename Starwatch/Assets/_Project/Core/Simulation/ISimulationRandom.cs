namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Injected randomness for game rules. Never use UnityEngine.Random in services.
    /// </summary>
    public interface ISimulationRandom
    {
        /// <summary>
        /// Returns a value in [minInclusive, maxExclusive).
        /// </summary>
        int Next(int minInclusive, int maxExclusive);

        /// <summary>
        /// Returns a float in [0, 1).
        /// </summary>
        float NextFloat();
    }
}
