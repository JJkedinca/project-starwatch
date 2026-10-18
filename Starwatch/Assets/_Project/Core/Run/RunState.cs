using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Core.Run
{
    /// <summary>
    /// Aggregate root for one campaign attempt. Holds seed plus the three
    /// persistence layers: universe, timeline, and archive.
    /// </summary>
    public sealed class RunState
    {
        public RunSeed Seed { get; }

        public UniverseState Universe { get; }

        public TimelineState Timeline { get; private set; }

        public RunArchive Archive { get; }

        public int RewindIndex { get; private set; }

        public ISimulationRandom Random { get; }

        /// <summary>
        /// Run-scoped entity id issuer. Shared with the active timeline so ids
        /// remain unique across rewind recreations.
        /// </summary>
        public EntityIdFactory EntityIds { get; }

        public RunState(
            RunSeed seed,
            UniverseState universe,
            TimelineState timeline,
            RunArchive archive,
            int rewindIndex,
            ISimulationRandom random,
            EntityIdFactory entityIds)
        {
            if (rewindIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rewindIndex), rewindIndex, "Rewind index must be non-negative.");
            }

            Seed = seed;
            Universe = universe ?? throw new ArgumentNullException(nameof(universe));
            Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            Archive = archive ?? throw new ArgumentNullException(nameof(archive));
            RewindIndex = rewindIndex;
            Random = random ?? throw new ArgumentNullException(nameof(random));
            EntityIds = entityIds ?? throw new ArgumentNullException(nameof(entityIds));
        }

        /// <summary>
        /// Swaps in a newly created timeline after a rewind and updates the index.
        /// </summary>
        public bool ReplaceTimeline(TimelineState timeline, int rewindIndex)
        {
            if (timeline == null || rewindIndex < 0)
            {
                return false;
            }

            Timeline = timeline;
            RewindIndex = rewindIndex;
            return true;
        }
    }
}
