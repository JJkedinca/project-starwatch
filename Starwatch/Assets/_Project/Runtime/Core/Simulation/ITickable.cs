namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Contract for systems driven by the managed simulation clock instead of
    /// their own Update loops.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Advances this system by the provided simulation delta time.
        /// </summary>
        void ManagedUpdate(float deltaTime);
    }
}
