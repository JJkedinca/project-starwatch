using System;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Builds a fresh timeline from scenario setup and archive unlocks without
    /// copying previous timeline power or aliasing scenario/archive collections.
    /// </summary>
    public sealed class TimelineFactory
    {
        /// <summary>
        /// Creates timeline-owned state for a new run or rewind.
        /// Uses the run-scoped entity id factory so ids stay unique across rewinds.
        /// </summary>
        public TimelineState Create(
            ScenarioState scenario,
            RunArchive archive,
            EntityIdFactory entityIds)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            if (entityIds == null)
            {
                throw new ArgumentNullException(nameof(entityIds));
            }

            _ = archive;

            var timeline = new TimelineState(entityIds);
            var startingEmpire = scenario.StartingEmpire;
            if (startingEmpire.CanPlaceStarterFleet)
            {
                timeline.TryAddFleet(
                    startingEmpire.StarterFleetClassId,
                    startingEmpire.StarterShipCount,
                    startingEmpire.HomeSystemId,
                    out _);
            }

            return timeline;
        }
    }
}
