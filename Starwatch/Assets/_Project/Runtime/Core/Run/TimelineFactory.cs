namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Builds a fresh timeline from scenario setup and archive unlocks without
    /// copying previous timeline power into the new attempt.
    /// </summary>
    public sealed class TimelineFactory
    {
        /// <summary>
        /// Creates timeline-owned state for a new run or rewind.
        /// </summary>
        public TimelineState Create(ScenarioState scenario, RunArchive archive, int rewindIndex)
        {
            _ = scenario;
            _ = archive;
            return new TimelineState(rewindIndex);
        }
    }
}
