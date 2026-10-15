using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Minimal authored starting empire data for Phase 0/1. Colony and resource
    /// fields arrive with the settlement slice; this only places home + fleet.
    /// </summary>
    public sealed class StartingEmpireDefinition
    {
        public ContentId HomeSystemId { get; }

        public ContentId StarterFleetClassId { get; }

        public int StarterShipCount { get; }

        /// <summary>
        /// True when home, fleet class, and ship count are all valid for placement.
        /// </summary>
        public bool CanPlaceStarterFleet =>
            StarterShipCount > 0
            && !HomeSystemId.IsEmpty
            && !StarterFleetClassId.IsEmpty;

        public StartingEmpireDefinition(ContentId homeSystemId, ContentId starterFleetClassId, int starterShipCount)
        {
            HomeSystemId = homeSystemId;
            StarterFleetClassId = starterFleetClassId;
            StarterShipCount = starterShipCount < 0 ? 0 : starterShipCount;
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
    }
}
