namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Aggregate root for one campaign attempt. Holds seed plus the three
    /// persistence layers: scenario, timeline, and archive.
    /// </summary>
    public sealed class RunState
    {
        public RunState(
            RunSeed seed,
            ScenarioState scenario,
            TimelineState timeline,
            RunArchive archive,
            int rewindIndex)
        {
            Seed = seed;
            Scenario = scenario;
            Timeline = timeline;
            Archive = archive;
            RewindIndex = rewindIndex;
        }

        public RunSeed Seed { get; }

        public ScenarioState Scenario { get; }

        public TimelineState Timeline { get; private set; }

        public RunArchive Archive { get; }

        public int RewindIndex { get; private set; }

        /// <summary>
        /// Swaps in a newly created timeline after a rewind and updates the index.
        /// </summary>
        public void ReplaceTimeline(TimelineState timeline, int rewindIndex)
        {
            Timeline = timeline;
            RewindIndex = rewindIndex;
        }
    }
}
