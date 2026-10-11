using EndlessHeros.Core.Run;

namespace EndlessHeros.Core.Persistence
{
    /// <summary>
    /// Persistence boundary for run state. Feature services depend on this
    /// abstraction so disk saves can replace the in-memory adapter later.
    /// </summary>
    public interface IRunStateStore
    {
        /// <summary>
        /// Stores the provided run state.
        /// </summary>
        void Save(RunState runState);

        /// <summary>
        /// Attempts to restore a previously stored run state.
        /// </summary>
        bool TryLoad(out RunState runState);

        /// <summary>
        /// Removes any stored run state.
        /// </summary>
        void Clear();
    }
}
