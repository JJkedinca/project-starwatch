namespace EndlessHeros.Core.Identifiers
{
    /// <summary>
    /// Issues monotonically increasing EntityIds for the lifetime of a run.
    /// Shared across timeline recreations so the same fleet keeps the same id.
    /// </summary>
    public sealed class EntityIdFactory
    {
        private long _next;

        public EntityIdFactory(long startAt = 0L)
        {
            _next = startAt;
        }

        /// <summary>
        /// Returns the next unused entity id.
        /// </summary>
        public EntityId Next()
        {
            _next++;
            return new EntityId(_next);
        }
    }
}
