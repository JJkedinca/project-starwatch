using System;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Archive;
using EndlessHeros.Core.Run.Universe;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Builds a fresh timeline from universe setup and archive unlocks without
    /// copying previous timeline power or aliasing universe/archive collections.
    /// </summary>
    internal sealed class TimelineFactory
    {
        /// <summary>
        /// Creates timeline-owned state for a new run or rewind.
        /// Uses the run-scoped entity id factory so ids stay unique across rewinds.
        /// </summary>
        public TimelineState Create(UniverseState universe, RunArchive archive, EntityIdFactory entityIds)
        {
            if (universe == null)
            {
                throw new ArgumentNullException(nameof(universe));
            }

            if (entityIds == null)
            {
                throw new ArgumentNullException(nameof(entityIds));
            }

            _ = archive;

            var timeline = new TimelineState(entityIds);
            var startingEmpire = universe.StartingEmpire;
            if (startingEmpire.CanPlaceStarterFleet)
            {
                timeline.TryAddFleet(
                    startingEmpire.StarterFleetClassId,
                    startingEmpire.StarterShipCount,
                    startingEmpire.HomeSystemId,
                    out _);
            }

            timeline.Resources.Grant(startingEmpire.StartingResources);

            return timeline;
        }
    }
}
