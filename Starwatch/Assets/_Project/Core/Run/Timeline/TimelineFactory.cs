using System;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Archive;
using EndlessHeros.Core.Run.Universe;
using UnityEngine;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// Builds a fresh timeline from universe setup and archive unlocks without
    /// copying previous timeline power or aliasing universe/archive collections.
    /// </summary>
    internal sealed class TimelineFactory
    {
        private readonly ObjectDatabaseSO _database;

        public TimelineFactory(ObjectDatabaseSO database = null)
        {
            _database = database;
        }

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
            SeedVerticalSliceHome(timeline, universe);

            return timeline;
        }

        /// <summary>
        /// The vertical slice starts with a level-1 colony and one of each catalog building. The stockpile is not charged.
        /// </summary>
        private void SeedVerticalSliceHome(TimelineState timeline, UniverseState universe)
        {
            if (_database == null
                || universe.ScenarioId != VerticalSliceScenarioFactory.ScenarioId
                || _database.SettlementCatalog == null)
            {
                return;
            }

            var home = ContentId.From("home-prime");
            var colony = new ColonyState(home, ColonyPresence.Colony);
            var buildings = _database.SettlementCatalog.Buildings;
            var placed = 0;
            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (building == null || building.Id.IsEmpty)
                {
                    continue;
                }

                colony.AddBuilding(new ColonyBuildingState(building.Id, 1));
                placed++;
            }

            if (placed == 0)
            {
                Debug.LogWarning("TimelineFactory found no buildings to place on the vertical-slice home colony.");
            }

            if (!timeline.TryAddColony(colony))
            {
                Debug.LogWarning("TimelineFactory could not place the vertical-slice home colony.");
            }
        }
    }
}
