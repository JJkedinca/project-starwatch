using EndlessHeros.Core.Run;

namespace EndlessHeros.Core.Persistence
{
    /// <summary>
    /// First-slice IRunStateStore that keeps run state in process memory only.
    /// </summary>
    public sealed class InMemoryRunStateStore : IRunStateStore
    {
        private RunState _stored;

        /// <summary>
        /// Replaces the in-memory stored run.
        /// </summary>
        public void Save(RunState runState)
        {
            _stored = runState;
        }

        /// <summary>
        /// Returns the stored run when one exists.
        /// </summary>
        public bool TryLoad(out RunState runState)
        {
            runState = _stored;
            return _stored != null;
        }

        /// <summary>
        /// Drops the stored run reference.
        /// </summary>
        public void Clear()
        {
            _stored = null;
        }
    }
}
