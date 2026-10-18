using System;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Universe;
using UnityEngine;

namespace ProjectStarwatch.Core.Run.Timeline
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
            SeedStartingSettlements(timeline, startingEmpire);

            return timeline;
        }

        /// <summary>
        /// Places the empire's authored settlements. The stockpile is not charged.
        /// </summary>
        private static void SeedStartingSettlements(TimelineState timeline, StartingEmpireDefinition empire)
        {
            var settlements = empire.StartingSettlements;
            for (var i = 0; i < settlements.Count; i++)
            {
                var settlement = settlements[i];
                if (settlement.PlanetId.IsEmpty)
                {
                    Debug.LogWarning("TimelineFactory skipped a starting settlement with no planet.");
                    continue;
                }

                var presence = settlement.IsColony ? ColonyPresence.Colony : ColonyPresence.Outpost;
                var colony = new ColonyState(settlement.PlanetId, presence);
                var buildings = settlement.Buildings;
                for (var buildingIndex = 0; buildingIndex < buildings.Count; buildingIndex++)
                {
                    var building = buildings[buildingIndex];
                    if (building.BuildingId.IsEmpty)
                    {
                        continue;
                    }

                    colony.AddBuilding(new ColonyBuildingState(building.BuildingId, building.Level));
                }

                if (!timeline.TryAddColony(colony))
                {
                    Debug.LogWarning(
                        $"TimelineFactory could not place the starting settlement on '{settlement.PlanetId.Value}'.");
                }
            }
        }
    }
}
