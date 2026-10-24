namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Presentation / real-time tick consumer. Opt in via <see cref="IUpdateManager"/>.
    /// </summary>
    public interface IFrameTickable
    {
        void ManagedUpdate(float unscaledDeltaTime);
    }
}
