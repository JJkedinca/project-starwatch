using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored starting empire: home, starter fleet, opening stockpile, and starting settlements.
    /// </summary>
    public sealed class StartingEmpireDefinition
    {
        public ContentId HomeSystemId { get; }

        public ContentId StarterFleetClassId { get; }

        public int StarterShipCount { get; }

        public StartingResources StartingResources { get; }

        public IReadOnlyList<StartingSettlement> StartingSettlements { get; }

        /// <summary>
        /// True when home, fleet class, and ship count are all valid for placement.
        /// </summary>
        public bool CanPlaceStarterFleet =>
            StarterShipCount > 0
            && !HomeSystemId.IsEmpty
            && !StarterFleetClassId.IsEmpty;

        public StartingEmpireDefinition(
            ContentId homeSystemId,
            ContentId starterFleetClassId,
            int starterShipCount,
            StartingResources startingResources = default,
            IReadOnlyList<StartingSettlement> startingSettlements = null)
        {
            HomeSystemId = homeSystemId;
            StarterFleetClassId = starterFleetClassId;
            StarterShipCount = starterShipCount < 0 ? 0 : starterShipCount;
            StartingResources = startingResources;
            StartingSettlements = Copy(startingSettlements);
        }

        /// <summary>
        /// Same home, fleet, and stockpile, with no settlements already placed.
        /// </summary>
        public StartingEmpireDefinition WithoutSettlements()
        {
            return new StartingEmpireDefinition(
                HomeSystemId,
                StarterFleetClassId,
                StarterShipCount,
                StartingResources);
        }

        /// <summary>
        /// Development default aligned with VerticalSliceScenarioFactory home.
        /// </summary>
        public static StartingEmpireDefinition CreateDefault()
        {
            return new StartingEmpireDefinition(
                ContentId.From("home-system"),
                ContentId.From("starter-screen"),
                starterShipCount: 3);
        }

        private static List<StartingSettlement> Copy(IReadOnlyList<StartingSettlement> source)
        {
            var copy = new List<StartingSettlement>();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    copy.Add(source[i]);
                }
            }

            return copy;
        }
    }
}
