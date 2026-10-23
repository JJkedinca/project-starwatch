namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Root real-time update hub. Frame tickables register while active; simulation is pumped separately.
    /// </summary>
    public interface IUpdateManager
    {
        void Register(IFrameTickable tickable);

        void Unregister(IFrameTickable tickable);
    }
}
