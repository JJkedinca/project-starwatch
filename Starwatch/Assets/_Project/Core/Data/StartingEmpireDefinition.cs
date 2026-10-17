using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored starting empire: home, starter fleet, and that faction's opening stockpile.
    /// </summary>
    public sealed class StartingEmpireDefinition
    {
        public ContentId HomeSystemId { get; }

        public ContentId StarterFleetClassId { get; }

        public int StarterShipCount { get; }

        public StartingResources StartingResources { get; }

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
            StartingResources startingResources = default)
        {
            HomeSystemId = homeSystemId;
            StarterFleetClassId = starterFleetClassId;
            StarterShipCount = starterShipCount < 0 ? 0 : starterShipCount;
            StartingResources = startingResources;
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
